// MoreRichTextLabel - drop-in C# control for Godot 4.7+
//
// Copy this single file into your project and attach MoreRichTextLabel.cs
// to a Control node. It supports:
//
//   [panel=res://path/to/stylebox.tres]content[/panel]
//   [panel]content[/panel]                  (uses DefaultPanelStyle)
//   [margin=10]content[/margin]
//
// Panels/margins are inline boxes: they hug their content and surrounding
// plain text continues on the same line, wrapping word by word when needed.
// Standard RichTextLabel BBCode, images, built-in effects and custom
// RichTextEffects are supported through internal RichTextLabels.
//
// The experimental static TextServer renderer lives in Experimental/ and is
// not required for this control.

using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MoreRichTextLabelLib;
// ---------------------------------------------------------------------------
// Document model
// ---------------------------------------------------------------------------

public enum DocBlockKind
{
	Sequence,
	Text,
	Panel,
	Margin,
	Break,
}

public abstract class DocBlock
{
	public DocBlockKind Kind { get; protected set; }
}

public sealed class DocSequence : DocBlock
{
	public List<DocBlock> Children { get; } = new();

	public DocSequence()
	{
		Kind = DocBlockKind.Sequence;
	}
}

public sealed class DocText : DocBlock
{
	public string Markup { get; set; } = "";

	// Used by the RichTextLabel-leaf implementations.
	public RichTextLabel? View { get; set; }

	public DocText()
	{
		Kind = DocBlockKind.Text;
	}
}

public sealed class DocPanel : DocBlock
{
	public StyleBox? Style { get; set; }
	public Color Modulate { get; set; } = Colors.White;
	public DocSequence Content { get; } = new();
	public Rect2 Rect { get; set; }
	public StyleBox? CachedModulatedStyle { get; set; }
	public StyleBox? CachedModulatedSource { get; set; }
	public Color CachedModulate { get; set; }

	public DocPanel()
	{
		Kind = DocBlockKind.Panel;
	}
}

public sealed class DocBreak : DocBlock
{
	public DocBreak()
	{
		Kind = DocBlockKind.Break;
	}
}
public sealed class DocMargin : DocBlock
{
	public float Left { get; set; }
	public float Top { get; set; }
	public float Right { get; set; }
	public float Bottom { get; set; }
	public DocSequence Content { get; } = new();

	public DocMargin()
	{
		Kind = DocBlockKind.Margin;
	}
}

// ---------------------------------------------------------------------------
// Inline text utilities
// ---------------------------------------------------------------------------


// ---------------------------------------------------------------------------
// BBCode document parser
// ---------------------------------------------------------------------------

/// <summary>
/// Splits a BBCode string into block nodes. Only [panel] and [margin] are custom block tags;
/// all other BBCode is preserved for the text renderer.
/// </summary>
public sealed class BbcodeDocumentParser
{
	private sealed class OpenTag
	{
		public string Name = "";
		public string RawOpen = "";
		public string RawClose = "";
	}

	private readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
	{
		"br", "hr", "lb", "rb",
	};

	private readonly Dictionary<string, StyleBox?> _styleCache = new(StringComparer.Ordinal);

	private readonly string _source;
	private readonly bool _trimBlockWhitespace;
	private readonly StringBuilder _buffer = new();
	private readonly List<OpenTag> _openTags = new();
	private List<OpenTag> _bufferStartTags = new();
	private int _pos;

	public BbcodeDocumentParser(string source, bool trimBlockWhitespace)
	{
		_source = source;
		_trimBlockWhitespace = trimBlockWhitespace;
	}

	public DocSequence ParseDocument()
	{
		var root = new DocSequence();
		ParseSequence(root, null);
		return root;
	}

	private void ParseSequence(DocSequence destination, string? stopTag)
	{
		while (_pos < _source.Length)
		{
			int open = _source.IndexOf('[', _pos);
			if (open < 0)
			{
				AppendTextSegment(_source.Substring(_pos), destination);
				_pos = _source.Length;
				break;
			}

			if (open > _pos)
			{
				AppendTextSegment(_source.Substring(_pos, open - _pos), destination);
				_pos = open;
			}

			int close = _source.IndexOf(']', _pos + 1);
			if (close < 0)
			{
				AppendTextSegment(_source.Substring(_pos), destination);
				_pos = _source.Length;
				break;
			}

			string raw = _source.Substring(_pos, close - _pos + 1);
			string inner = _source.Substring(_pos + 1, close - _pos - 1);
			_pos = close + 1;

			ParseTag(inner, out string name, out bool isClosing, out string value, out string rest);

			if (name.Equals("panel", StringComparison.OrdinalIgnoreCase) ||
				name.Equals("margin", StringComparison.OrdinalIgnoreCase))
			{
				if (isClosing)
				{
					if (stopTag != null && name.Equals(stopTag, StringComparison.OrdinalIgnoreCase))
					{
						FlushText(destination);
						return;
					}

					// Unmatched custom closing tag: keep it as literal text.
					_buffer.Append(raw);
					continue;
				}

				FlushText(destination);

				if (name.Equals("panel", StringComparison.OrdinalIgnoreCase))
				{
					ParsePanelRest(rest, out string stylePath, out Color modulate);
					var panel = new DocPanel { Style = LoadStyle(stylePath), Modulate = modulate };
					ParseSequence(panel.Content, "panel");
					destination.Children.Add(panel);
				}
				else
				{
					var margin = new DocMargin();
					ApplyMargin(margin, value);
					ParseSequence(margin.Content, "margin");
					destination.Children.Add(margin);
				}

				continue;
			}

			AppendStandardTag(name, isClosing, raw);
		}

		FlushText(destination);
	}

	private void AppendTextSegment(string text, DocSequence destination)
	{
		int start = 0;

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];

			if (c != '\n' && c != '\r')
			{
				continue;
			}

			if (i > start)
			{
				_buffer.Append(text, start, i - start);
			}

			if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
			{
				i++;
			}

			FlushText(destination);
			destination.Children.Add(new DocBreak());
			start = i + 1;
		}

		if (start < text.Length)
		{
			_buffer.Append(text, start, text.Length - start);
		}
	}
	private void AppendStandardTag(string name, bool isClosing, string raw)
	{
		if (isClosing)
		{
			_buffer.Append(raw);

			for (int i = _openTags.Count - 1; i >= 0; i--)
			{
				if (_openTags[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					_openTags.RemoveAt(i);
					break;
				}
			}

			return;
		}

		_buffer.Append(raw);

		if (!VoidTags.Contains(name))
		{
			_openTags.Add(new OpenTag
			{
				Name = name,
				RawOpen = raw,
				RawClose = $"[/{name}]",
			});
		}
	}

	private void FlushText(DocSequence destination)
	{
		if (_buffer.Length == 0)
		{
			_bufferStartTags = new List<OpenTag>(_openTags);
			return;
		}

		string raw = _buffer.ToString();
		_buffer.Clear();

		if (_trimBlockWhitespace)
		{
			raw = raw.Trim('\t', '\r', '\n');
		}

		if (raw.Length > 0)
		{
			var markup = new StringBuilder(raw.Length + 32);

			foreach (OpenTag tag in _bufferStartTags)
			{
				markup.Append(tag.RawOpen);
			}

			markup.Append(raw);

			for (int i = _openTags.Count - 1; i >= 0; i--)
			{
				markup.Append(_openTags[i].RawClose);
			}

			destination.Children.Add(new DocText { Markup = markup.ToString() });
		}

		_bufferStartTags = new List<OpenTag>(_openTags);
	}

	private void ParseTag(string inner, out string name, out bool isClosing, out string value, out string rest)
	{
		name = "";
		value = "";
		rest = "";
		isClosing = inner.StartsWith('/');

		int start = isClosing ? 1 : 0;
		int end = start;

		while (end < inner.Length && inner[end] != '=' && !char.IsWhiteSpace(inner[end]))
		{
			end++;
		}

		name = inner.Substring(start, end - start).Trim();
		rest = inner.Substring(end).Trim();

		if (!isClosing)
		{
			int equals = inner.IndexOf('=');
			if (equals >= 0)
			{
				value = inner.Substring(equals + 1).Trim();
			}
		}
	}

	private void ParsePanelRest(string rest, out string stylePath, out Color modulate)
	{
		stylePath = "";
		modulate = Colors.White;

		string body = rest.Trim();

		if (body.StartsWith("="))
		{
			body = body.Substring(1).Trim();
		}

		body = StripPanelKey(body);

		int modIndex = body.IndexOf("modulate=", StringComparison.OrdinalIgnoreCase);

		if (modIndex >= 0)
		{
			string pathPart = body.Substring(0, modIndex).TrimEnd(' ', ',');
			string modPart = body.Substring(modIndex + "modulate=".Length).Trim();

			stylePath = CleanPanelPath(pathPart);

			int tokenEnd = modPart.IndexOfAny(new[] { ' ', ',', '\t' });
			string token = tokenEnd >= 0 ? modPart.Substring(0, tokenEnd) : modPart;

			if (!string.IsNullOrWhiteSpace(token))
			{
				modulate = Color.FromString(token, Colors.White);
			}
		}
		else
		{
			stylePath = CleanPanelPath(body);
		}
	}

	private string StripPanelKey(string body)
	{
		string[] keys = { "panel=", "path=", "style=", "stylebox=" };

		foreach (string key in keys)
		{
			if (body.StartsWith(key, StringComparison.OrdinalIgnoreCase))
			{
				return body.Substring(key.Length).Trim();
			}
		}

		return body;
	}

	private string CleanPanelPath(string path)
	{
		string result = path.Trim();

		if (result.StartsWith("="))
		{
			result = result.Substring(1).Trim();
		}

		result = result.TrimEnd(',', ' ').Trim();
		return StripPanelKey(result);
	}
	private StyleBox? LoadStyle(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}

		if (_styleCache.TryGetValue(path, out StyleBox? cached))
		{
			return cached;
		}

		StyleBox? style = null;

		try
		{
			if (ResourceLoader.Exists(path))
			{
				style = ResourceLoader.Load<StyleBox>(path);
			}
		}
		catch (Exception exception)
		{
			GD.PushWarning($"MoreRichTextLabel: failed to load StyleBox '{path}': {exception.Message}");
		}

		if (style == null)
		{
			GD.PushWarning($"MoreRichTextLabel: StyleBox not found or wrong type: {path}");
		}

		_styleCache[path] = style;
		return style;
	}

	private void ApplyMargin(DocMargin margin, string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			SetMargin(margin, 0);
			return;
		}

		string[] parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		if (!TryFloat(parts, 0, out float first))
		{
			GD.PushWarning($"MoreRichTextLabel: invalid margin value '{value}'");
			SetMargin(margin, 0);
			return;
		}

		switch (parts.Length)
		{
			case 1:
				SetMargin(margin, first);
				break;

			case 2 when TryFloat(parts, 1, out float second):
				margin.Left = first;
				margin.Right = first;
				margin.Top = second;
				margin.Bottom = second;
				break;

			case 4 when TryFloat(parts, 1, out float top) &&
						TryFloat(parts, 2, out float right) &&
						TryFloat(parts, 3, out float bottom):
				margin.Left = first;
				margin.Top = top;
				margin.Right = right;
				margin.Bottom = bottom;
				break;

			default:
				GD.PushWarning($"MoreRichTextLabel: invalid margin value '{value}'");
				SetMargin(margin, first);
				break;
		}
	}

	private bool TryFloat(string[] parts, int index, out float value)
	{
		return float.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}

	private void SetMargin(DocMargin margin, float value)
	{
		margin.Left = value;
		margin.Top = value;
		margin.Right = value;
		margin.Bottom = value;
	}
}

// ---------------------------------------------------------------------------
// MoreRichTextLabel control
// ---------------------------------------------------------------------------

public enum InlineRowAlignment
{
	Top,
	Middle,
	Bottom,
}

/// <summary>
/// C# implementation using one RichTextLabel per text block and a custom vertical block layout.
/// Panels and margins are drawn/laid out by this control. The RichTextLabel leaves are internal
/// children so the full RichTextLabel feature set (including RichTextEffects) remains available.
/// </summary>
[Tool]
[GlobalClass]
public partial class MoreRichTextLabel : Control
{
	[Export(PropertyHint.MultilineText)]
	public string Text
	{
		get => _text;
		set
		{
			string next = value ?? "";
			if (_text == next)
			{
				return;
			}

			_text = next;
			if (IsInsideTree())
			{
				Rebuild();
			}
		}
	}

	[Export]
	public StyleBox? DefaultPanelStyle
	{
		get => _defaultPanelStyle;
		set
		{
			if (_defaultPanelStyle == value)
			{
				return;
			}

			_defaultPanelStyle = value;

			if (IsInsideTree())
			{
				Relayout();
				QueueRedraw();
			}
		}
	}

	[Export]
	public bool SelectionEnabled
	{
		get => _selectionEnabled;
		set
		{
			if (_selectionEnabled == value)
			{
				return;
			}

			_selectionEnabled = value;
			foreach (DocText text in _textNodes)
			{
				if (text.View != null)
				{
					ConfigureLeafInput(text.View);
				}
			}

			foreach (SplitTextViews split in _splitViews.Values)
			{
				if (split.Head != null && IsInstanceValid(split.Head))
				{
					ConfigureLeafInput(split.Head);
				}

				if (split.Tail != null && IsInstanceValid(split.Tail))
				{
					ConfigureLeafInput(split.Tail);
				}
			}
		}
	}

	[Export]
	public bool TrimBlockWhitespace
	{
		get => _trimBlockWhitespace;
		set
		{
			if (_trimBlockWhitespace == value)
			{
				return;
			}

			_trimBlockWhitespace = value;
			if (IsInsideTree())
			{
				Rebuild();
			}
		}
	}

	[Export]
	public Godot.Collections.Array<RichTextEffect> CustomEffects
	{
		get => _customEffects;
		set
		{
			_customEffects = value ?? new Godot.Collections.Array<RichTextEffect>();
			if (IsInsideTree())
			{
				Rebuild();
			}
		}
	}

	[Export]
	public InlineRowAlignment RowAlignment
	{
		get => _rowAlignment;
		set
		{
			if (_rowAlignment == value)
			{
				return;
			}

			_rowAlignment = value;
			if (IsInsideTree())
			{
				Relayout();
				QueueRedraw();
			}
		}
	}

	private readonly StyleBoxEmpty _emptyStyle = new();

	private string _text = "";
	private InlineRowAlignment _rowAlignment = InlineRowAlignment.Top;
	private readonly List<RichTextLabel> _placedViews = new();
	private readonly Dictionary<DocText, SplitTextViews> _splitViews = new();

	private sealed class SplitTextViews(RichTextLabel head, RichTextLabel tail)
	{
		public RichTextLabel Head { get; } = head;
		public RichTextLabel Tail { get; } = tail;
	}
	private StyleBox? _defaultPanelStyle;
	private bool _selectionEnabled;
	private bool _trimBlockWhitespace = true;
	private Godot.Collections.Array<RichTextEffect> _customEffects = new();
	private DocSequence? _root;
	private readonly List<DocText> _textNodes = new();
	private readonly List<DocPanel> _panels = new();
	private readonly Dictionary<DocText, float> _preferredTextWidths = new();
	private readonly HashSet<StyleBox> _connectedStyles = new();
	private readonly List<StyleBox> _connectedStyleList = new();
	private float _contentHeight;
	private bool _inLayout;

	public float ContentHeight => _contentHeight;
	public int TextBlockCount => _textNodes.Count;

	public override void _Ready()
	{
		Rebuild();
	}

	public override void _ExitTree()
	{
		DisconnectStyleSignals();
		_inLayout = false;
	}

	public void Rebuild()
	{
		if (!IsInsideTree())
		{
			return;
		}

		foreach (DocText text in _textNodes)
		{
			if (text.View != null && IsInstanceValid(text.View))
			{
				RemoveChild(text.View);
				text.View.Free();
				text.View = null;
			}
		}

		_textNodes.Clear();
		_preferredTextWidths.Clear();

		foreach (SplitTextViews split in _splitViews.Values)
		{
			foreach (RichTextLabel view in new[] { split.Head, split.Tail })
			{
				if (view != null && IsInstanceValid(view))
				{
					RemoveChild(view);
					view.Free();
				}
			}
		}

		_splitViews.Clear();
		_root = new BbcodeDocumentParser(_text, TrimBlockWhitespace).ParseDocument();
		CreateViews(_root);
		Relayout();
	}

	public void SetTextForced(string value)
	{
		_text = value ?? "";
		Rebuild();
	}

	public void ForceRelayout()
	{
		Relayout(Size.X);
	}

	public void ForceRelayoutAtWidth(float width)
	{
		Relayout(width);
	}

	private void CreateViews(DocSequence sequence)
	{
		foreach (DocBlock block in sequence.Children)
		{
			switch (block)
			{
				case DocSequence childSequence:
					CreateViews(childSequence);
					break;

				case DocText textBlock:
					CreateLeaf(textBlock);
					break;

				case DocPanel panel:
					CreateViews(panel.Content);
					break;

				case DocMargin margin:
					CreateViews(margin.Content);
					break;

				case DocBreak:
					break;
			}
		}
	}

	private void CreateLeaf(DocText block)
	{
		var leaf = new RichTextLabel
		{
			BbcodeEnabled = true,
			FitContent = false,
			ScrollActive = false,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			AutowrapTrimFlags = TextServer.LineBreakFlag.TrimStartEdgeSpaces,
			ClipContents = false,
		};

		leaf.AddThemeStyleboxOverride("normal", _emptyStyle);

		foreach (RichTextEffect effect in _customEffects)
		{
			if (effect != null)
			{
				leaf.InstallEffect(effect);
			}
		}

		ConfigureLeafInput(leaf);
		AddChild(leaf, false, InternalMode.Front);
		leaf.Text = block.Markup;

		block.View = leaf;
		_textNodes.Add(block);
	}

	private void ConfigureLeafInput(RichTextLabel leaf)
	{
		leaf.SelectionEnabled = _selectionEnabled;
		leaf.MouseFilter = _selectionEnabled
			? MouseFilterEnum.Stop
			: MouseFilterEnum.Ignore;
		leaf.FocusMode = _selectionEnabled
			? FocusModeEnum.Click
			: FocusModeEnum.None;
	}

	private void Relayout() => Relayout(Size.X);

	private void Relayout(float width)
	{
		if (_inLayout)
		{
			return;
		}

		if (_root == null)
		{
			return;
		}

		if (HasInvalidViews())
		{
			Callable.From(Rebuild).CallDeferred();
			return;
		}

		HideAllTextViews();

		_inLayout = true;
		try
		{
			_panels.Clear();
			_placedViews.Clear();

			if (float.IsNaN(width) || width <= 0.0f)
			{
				width = 1.0f;
			}

			_contentHeight = LayoutSequence(_root, 0.0f, 0.0f, width);
		}
		finally
		{
			_inLayout = false;
		}

		UpdateStyleConnections();
		UpdateMinimumSize();
		QueueRedraw();
	}

	private readonly struct RowItem
	{
		public readonly int TextStart;
		public readonly int TextEnd;
		public readonly int PanelStart;
		public readonly int PanelEnd;
		public readonly float Height;

		public RowItem(int textStart, int textEnd, int panelStart, int panelEnd, float height)
		{
			TextStart = textStart;
			TextEnd = textEnd;
			PanelStart = panelStart;
			PanelEnd = panelEnd;
			Height = height;
		}
	}

	private bool HasInvalidViews()
	{
		foreach (DocText text in _textNodes)
		{
			if (text.View == null || !IsInstanceValid(text.View))
			{
				return true;
			}
		}

		foreach (SplitTextViews split in _splitViews.Values)
		{
			if (split.Head == null || split.Tail == null ||
				!IsInstanceValid(split.Head) || !IsInstanceValid(split.Tail))
			{
				return true;
			}
		}

		return false;
	}

	private void RelayoutDeferred()
	{
		if (!IsInsideTree())
		{
			return;
		}

		_inLayout = false;

		if (_root == null || HasInvalidViews())
		{
			Rebuild();
			return;
		}

		Relayout();
	}

	private void RebuildDeferred()
	{
		if (IsInsideTree())
		{
			Rebuild();
		}
	}
	private void HideAllTextViews()
	{
		foreach (DocText text in _textNodes)
		{
			if (text.View != null && IsInstanceValid(text.View))
			{
				text.View.Visible = false;
			}
		}

		foreach (SplitTextViews split in _splitViews.Values)
		{
			if (split.Head != null && IsInstanceValid(split.Head))
			{
				split.Head.Visible = false;
			}

			if (split.Tail != null && IsInstanceValid(split.Tail))
			{
				split.Tail.Visible = false;
			}
		}
	}

	private SplitTextViews GetSplitViews(DocText text)
	{
		if (_splitViews.TryGetValue(text, out SplitTextViews? split))
		{
			return split;
		}

		split = new SplitTextViews(CreateAuxiliaryLeaf(), CreateAuxiliaryLeaf());

		_splitViews[text] = split;
		return split;
	}

	private RichTextLabel CreateAuxiliaryLeaf()
	{
		var leaf = new RichTextLabel
		{
			BbcodeEnabled = true,
			FitContent = false,
			ScrollActive = false,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			AutowrapTrimFlags = TextServer.LineBreakFlag.TrimStartEdgeSpaces,
			ClipContents = false,
			Visible = false,
		};

		leaf.AddThemeStyleboxOverride("normal", _emptyStyle);

		foreach (RichTextEffect effect in _customEffects)
		{
			if (effect != null)
			{
				leaf.InstallEffect(effect);
			}
		}

		ConfigureLeafInput(leaf);
		AddChild(leaf, false, InternalMode.Front);
		return leaf;
	}

	private bool IsCustomEffectTag(string name)
	{
		foreach (RichTextEffect effect in _customEffects)
		{
			if (effect == null)
			{
				continue;
			}

			Variant bbcode = effect.Get("bbcode");
			if (bbcode.VariantType == Variant.Type.String &&
				name.Equals(bbcode.AsString(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private float GetViewPreferredWidth(RichTextLabel view)
	{
		if (!IsInstanceValid(view))
		{
			return 0.0f;
		}

		TextServer.AutowrapMode previousAutowrap = view.AutowrapMode;
		Vector2 previousSize = view.Size;

		view.AutowrapMode = TextServer.AutowrapMode.Off;
		view.Size = new Vector2(100000.0f, 1.0f);
		float preferred = view.GetContentWidth();

		view.AutowrapMode = previousAutowrap;
		view.Size = previousSize;
		return preferred;
	}

	private float PlaceTextView(RichTextLabel view, float x, float y, float width)
	{
		if (!IsInstanceValid(view))
		{
			return 0.0f;
		}

		view.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		view.Position = new Vector2(x, y);
		view.Size = new Vector2(width, 1.0f);
		float height = view.GetContentHeight();
		view.Size = new Vector2(width, height);
		view.Visible = true;
		_placedViews.Add(view);
		return height;
	}

	private float GetEmptyLineHeight()
	{
		Font? font = GetThemeFont("normal_font", "RichTextLabel");
		int fontSize = GetThemeFontSize("normal_font_size", "RichTextLabel");

		if (fontSize <= 0)
		{
			fontSize = 16;
		}

		if (font != null)
		{
			return font.GetHeight(fontSize) + GetThemeConstant("line_separation", "RichTextLabel");
		}

		return fontSize;
	}

	private float LayoutSequence(DocSequence sequence, float x, float y, float width)
	{
		float cursorY = y;
		float lineX = x;
		float lineHeight = 0.0f;
		bool hasLine = false;
		bool lineHasWrappedText = false;
		var rowItems = new List<RowItem>();

		foreach (DocBlock block in sequence.Children)
		{
			if (block is DocBreak)
			{
				if (hasLine)
				{
					ApplyRowAlignment(rowItems, lineHeight);
					rowItems.Clear();
					cursorY += lineHeight;
				}
				else
				{
					cursorY += GetEmptyLineHeight();
				}

				lineX = x;
				lineHeight = 0.0f;
				hasLine = false;
				lineHasWrappedText = false;
				continue;
			}

			if (block is DocText blockText && IsBlockText(blockText))
			{
				if (hasLine)
				{
					ApplyRowAlignment(rowItems, lineHeight);
					rowItems.Clear();
					cursorY += lineHeight;
					lineX = x;
					lineHeight = 0.0f;
					hasLine = false;
					lineHasWrappedText = false;
				}

				cursorY += PlaceText(blockText, x, cursorY, width);
				continue;
			}

			float preferred = GetPreferredWidth(block);
			float itemWidth = Mathf.Min(preferred, width);

			if (itemWidth <= 0.01f)
			{
				continue;
			}

			if (block is DocPanel && hasLine && lineHasWrappedText)
			{
				ApplyRowAlignment(rowItems, lineHeight);
				rowItems.Clear();
				cursorY += lineHeight;
				lineX = x;
				lineHeight = 0.0f;
				hasLine = false;
				lineHasWrappedText = false;
			}

			if (hasLine && lineX + itemWidth > x + width)
			{
				if (block is DocText splitText &&
					TryPlaceSplitText(splitText, x, width, rowItems, ref lineX, ref cursorY, ref lineHeight, ref hasLine))
				{
					lineHasWrappedText = true;
					continue;
				}

				ApplyRowAlignment(rowItems, lineHeight);
				rowItems.Clear();
				cursorY += lineHeight;
				lineX = x;
				lineHeight = 0.0f;
				hasLine = false;
				lineHasWrappedText = false;
			}

			int textStart = _placedViews.Count;
			int panelStart = _panels.Count;
			float itemHeight = PlaceBlock(block, lineX, cursorY, itemWidth);
			rowItems.Add(new RowItem(textStart, _placedViews.Count, panelStart, _panels.Count, itemHeight));

			if (block is DocText placedText && placedText.View != null && placedText.View.GetLineCount() > 1)
			{
				lineHasWrappedText = true;
			}

			lineX += itemWidth;
			lineHeight = Mathf.Max(lineHeight, itemHeight);
			hasLine = true;
		}

		if (hasLine)
		{
			ApplyRowAlignment(rowItems, lineHeight);
			cursorY += lineHeight;
		}

		return cursorY - y;
	}

	private void ApplyRowAlignment(List<RowItem> rowItems, float lineHeight)
	{
		foreach (RowItem item in rowItems)
		{
			float offset = GetRowOffset(item.Height, lineHeight);
			if (offset == 0.0f)
			{
				continue;
			}

			for (int i = item.TextStart; i < item.TextEnd; i++)
			{
				RichTextLabel view = _placedViews[i];
				if (IsInstanceValid(view))
				{
					view.Position = view.Position + new Vector2(0.0f, offset);
				}
			}

			for (int i = item.PanelStart; i < item.PanelEnd; i++)
			{
				DocPanel panel = _panels[i];
				panel.Rect = new Rect2(
					panel.Rect.Position + new Vector2(0.0f, offset),
					panel.Rect.Size);
			}
		}
	}

	private float GetRowOffset(float itemHeight, float lineHeight)
	{
		return _rowAlignment switch
		{
			InlineRowAlignment.Middle => (lineHeight - itemHeight) * 0.5f,
			InlineRowAlignment.Bottom => lineHeight - itemHeight,
			_ => 0.0f,
		};
	}

	private float PlaceBlock(DocBlock block, float x, float y, float width)
	{
		switch (block)
		{
			case DocText text:
				return PlaceText(text, x, y, width);

			case DocMargin margin:
				{
					float contentX = x + margin.Left;
					float contentY = y + margin.Top;
					float contentWidth = Mathf.Max(1.0f, width - margin.Left - margin.Right);
					float height = LayoutSequence(margin.Content, contentX, contentY, contentWidth);
					return height + margin.Top + margin.Bottom;
				}

			case DocPanel panel:
				{
					StyleBox style = GetPanelStyle(panel);
					float left = style.GetMargin(Side.Left);
					float top = style.GetMargin(Side.Top);
					float right = style.GetMargin(Side.Right);
					float bottom = style.GetMargin(Side.Bottom);
					float contentWidth = Mathf.Max(1.0f, width - left - right);

					panel.Rect = new Rect2(x, y, width, 0.0f);
					_panels.Add(panel);

					float contentHeight = LayoutSequence(panel.Content, x + left, y + top, contentWidth);
					float height = contentHeight + top + bottom;
					panel.Rect = new Rect2(x, y, width, height);
					return height;
				}

			case DocSequence sequence:
				return LayoutSequence(sequence, x, y, width);

			default:
				return 0.0f;
		}
	}

	private float PlaceText(DocText text, float x, float y, float width)
	{
		RichTextLabel? view = text.View;
		if (view == null || !IsInstanceValid(view))
		{
			return 0.0f;
		}

		return PlaceTextView(view, x, y, width);
	}

	private float GetPreferredWidth(DocBlock block)
	{
		switch (block)
		{
			case DocText text:
				return GetTextPreferredWidth(text);

			case DocPanel panel:
				{
					StyleBox style = GetPanelStyle(panel);
					return style.GetMargin(Side.Left) +
						   GetSequencePreferredWidth(panel.Content) +
						   style.GetMargin(Side.Right);
				}

			case DocMargin margin:
				return margin.Left + GetSequencePreferredWidth(margin.Content) + margin.Right;

			case DocSequence sequence:
				return GetSequencePreferredWidth(sequence);

			case DocBreak:
				return 0.0f;

			default:
				return 0.0f;
		}
	}

	private float GetSequencePreferredWidth(DocSequence sequence)
	{
		float width = 0.0f;

		foreach (DocBlock block in sequence.Children)
		{
			width += GetPreferredWidth(block);
		}

		return width;
	}

	private float GetTextPreferredWidth(DocText text)
	{
		if (_preferredTextWidths.TryGetValue(text, out float cached))
		{
			return cached;
		}

		RichTextLabel? view = text.View;
		if (view == null || !IsInstanceValid(view))
		{
			return 0.0f;
		}

		float preferred = GetViewPreferredWidth(view);
		_preferredTextWidths[text] = preferred;
		return preferred;
	}
	private bool TryPlaceSplitText(
		DocText text,
		float x,
		float width,
		List<RowItem> rowItems,
		ref float lineX,
		ref float cursorY,
		ref float lineHeight,
		ref bool hasLine)
	{
		if (!TryGetSplitFragments(text.Markup, out List<string> words) || words.Count < 2)
		{
			return false;
		}

		SplitTextViews split = GetSplitViews(text);
		RichTextLabel head = split.Head;
		RichTextLabel tail = split.Tail;

		float remaining = (x + width) - lineX;
		if (remaining <= 0.0f)
		{
			return false;
		}

		string headText = "";
		float headWidth = 0.0f;
		int fitCount = 0;

		for (int i = 0; i < words.Count; i++)
		{
			string candidate = headText + words[i];
			head.Text = candidate;
			float candidateWidth = GetViewPreferredWidth(head);

			if (candidateWidth > remaining)
			{
				break;
			}

			headText = candidate;
			headWidth = candidateWidth;
			fitCount++;
		}

		if (fitCount == 0 || fitCount >= words.Count)
		{
			return false;
		}

		head.Text = headText;
		int headTextStart = _placedViews.Count;
		int headPanelStart = _panels.Count;
		float headHeight = PlaceTextView(head, lineX, cursorY, headWidth);
		rowItems.Add(new RowItem(headTextStart, _placedViews.Count, headPanelStart, _panels.Count, headHeight));

		float rowHeight = Mathf.Max(lineHeight, headHeight);
		ApplyRowAlignment(rowItems, rowHeight);
		rowItems.Clear();
		cursorY += rowHeight;
		lineX = x;
		lineHeight = 0.0f;
		hasLine = false;

		var tailBuilder = new StringBuilder();
		for (int i = fitCount; i < words.Count; i++)
		{
			tailBuilder.Append(words[i]);
		}

		string tailText = tailBuilder.ToString().TrimStart(' ', '\t');
		if (tailText.Length == 0)
		{
			return true;
		}

		tail.Text = tailText;
		float tailWidth = Mathf.Min(GetViewPreferredWidth(tail), width);
		if (tailWidth <= 0.01f)
		{
			tailWidth = width;
		}

		int tailTextStart = _placedViews.Count;
		int tailPanelStart = _panels.Count;
		float tailHeight = PlaceTextView(tail, x, cursorY, tailWidth);
		rowItems.Add(new RowItem(tailTextStart, _placedViews.Count, tailPanelStart, _panels.Count, tailHeight));
		lineX = x + tailWidth;
		lineHeight = tailHeight;
		hasLine = true;
		return true;
	}
	private bool TryGetSplitFragments(string markup, out List<string> fragments)
	{
		fragments = new List<string>();

		if (markup.IndexOf('\n') >= 0 || markup.IndexOf('\r') >= 0)
		{
			return false;
		}

		var openNames = new List<string>();
		var openRaws = new List<string>();
		string pending = "";
		int i = 0;

		while (i < markup.Length)
		{
			char c = markup[i];

			if (c == '[')
			{
				int close = markup.IndexOf(']', i + 1);
				if (close < 0)
				{
					return false;
				}

				string raw = markup.Substring(i, close - i + 1);
				string inner = markup.Substring(i + 1, close - i - 1);
				bool closing = inner.StartsWith('/');
				int nameStart = closing ? 1 : 0;
				int nameEnd = nameStart;

				while (nameEnd < inner.Length && inner[nameEnd] != '=' && !char.IsWhiteSpace(inner[nameEnd]))
				{
					nameEnd++;
				}

				string name = inner.Substring(nameStart, nameEnd - nameStart).Trim().ToLowerInvariant();

				if (!closing && name == "img")
				{
					if (pending.Trim().Length > 0 || (i > 0 && !char.IsWhiteSpace(markup[i - 1])))
					{
						return false;
					}

					int imageClose = markup.IndexOf("[/img]", close + 1, StringComparison.OrdinalIgnoreCase);
					if (imageClose < 0)
					{
						return false;
					}

					int imageEnd = imageClose + "[/img]".Length;
					FlushSplitFragment(pending + markup.Substring(i, imageEnd - i), openNames, openRaws, fragments);
					pending = "";
					i = imageEnd;
					continue;
				}

				if (!IsSafeInlineTag(name))
				{
					return false;
				}

				if (closing)
				{
					if (openNames.Count == 0 || openNames[openNames.Count - 1] != name)
					{
						return false;
					}

					if (pending.Length > 0)
					{
						FlushSplitFragment(pending, openNames, openRaws, fragments);
						pending = "";
					}

					openNames.RemoveAt(openNames.Count - 1);
					openRaws.RemoveAt(openRaws.Count - 1);
				}
				else
				{
					if (pending.Trim().Length > 0)
					{
						return false;
					}

					openNames.Add(name);
					openRaws.Add(raw);
				}

				i = close + 1;
				continue;
			}

			if (char.IsWhiteSpace(c))
			{
				if (pending.Trim().Length > 0)
				{
					FlushSplitFragment(pending, openNames, openRaws, fragments);
					pending = "";
				}

				pending += c;
			}
			else
			{
				pending += c;
			}

			i++;
		}

		if (openNames.Count != 0)
		{
			return false;
		}

		if (pending.Length > 0)
		{
			FlushSplitFragment(pending, openNames, openRaws, fragments);
		}

		return fragments.Count > 1;
	}

	private void FlushSplitFragment(string text, List<string> openNames, List<string> openRaws, List<string> fragments)
	{
		var builder = new StringBuilder();

		foreach (string raw in openRaws)
		{
			builder.Append(raw);
		}

		builder.Append(text);

		for (int i = openNames.Count - 1; i >= 0; i--)
		{
			builder.Append("[/").Append(openNames[i]).Append(']');
		}

		fragments.Add(builder.ToString());
	}
	private bool IsSafeInlineTag(string name)
	{
		return name switch
		{
			"b" or "i" or "u" or "s" or
			"color" or "bgcolor" or "fgcolor" or
			"font" or "font_size" or
			"outline_size" or "outline_color" or
			"meta" or "hint" or "language" or "code" or
			"wave" or "shake" or "tornado" or "rainbow" or "pulse" or "fade" => true,
			_ => IsCustomEffectTag(name),
		};
	}

	private bool IsBlockText(DocText text)
	{
		return text.Markup.IndexOf('\n') >= 0 || text.Markup.IndexOf('\r') >= 0;
	}
	private StyleBox GetPanelStyle(DocPanel panel)
	{
		return panel.Style ?? _defaultPanelStyle ?? _emptyStyle;
	}

	private StyleBox GetDrawPanelStyle(DocPanel panel)
	{
		StyleBox style = GetPanelStyle(panel);

		if (panel.Modulate == Colors.White)
		{
			return style;
		}

		if (panel.CachedModulatedStyle != null &&
			panel.CachedModulatedSource == style &&
			panel.CachedModulate == panel.Modulate)
		{
			return panel.CachedModulatedStyle;
		}

		StyleBox? modulated = CreateModulatedStyle(style, panel.Modulate);
		panel.CachedModulatedSource = style;
		panel.CachedModulate = panel.Modulate;
		panel.CachedModulatedStyle = modulated;
		return modulated ?? style;
	}

	private StyleBox? CreateModulatedStyle(StyleBox style, Color modulate)
	{
		if (style is StyleBoxFlat flatSource)
		{
			var flat = (StyleBoxFlat)flatSource.Duplicate();
			flat.BgColor = MultiplyColor(flat.BgColor, modulate);
			flat.BorderColor = MultiplyColor(flat.BorderColor, modulate);
			flat.ShadowColor = MultiplyColor(flat.ShadowColor, modulate);
			return flat;
		}

		if (style is StyleBoxTexture textureSource)
		{
			var texture = (StyleBoxTexture)textureSource.Duplicate();
			texture.ModulateColor = MultiplyColor(texture.ModulateColor, modulate);
			return texture;
		}

		if (style is StyleBoxLine lineSource)
		{
			var line = (StyleBoxLine)lineSource.Duplicate();
			line.Color = MultiplyColor(line.Color, modulate);
			return line;
		}

		return null;
	}

	private Color MultiplyColor(Color color, Color modulate)
	{
		return new Color(
			color.R * modulate.R,
			color.G * modulate.G,
			color.B * modulate.B,
			color.A * modulate.A);
	}

	private void InvalidateModulatedStyles()
	{
		foreach (DocPanel panel in _panels)
		{
			panel.CachedModulatedStyle = null;
			panel.CachedModulatedSource = null;
		}
	}
	private void UpdateStyleConnections()
	{
		var needed = new HashSet<StyleBox>();

		if (_defaultPanelStyle != null)
		{
			needed.Add(_defaultPanelStyle);
		}

		foreach (DocPanel panel in _panels)
		{
			StyleBox style = GetPanelStyle(panel);
			if (style != _emptyStyle)
			{
				needed.Add(style);
			}
		}

		for (int i = _connectedStyleList.Count - 1; i >= 0; i--)
		{
			StyleBox style = _connectedStyleList[i];
			if (!needed.Contains(style))
			{
				if (IsInstanceValid(style))
				{
					style.Changed -= OnStyleBoxChanged;
				}

				_connectedStyleList.RemoveAt(i);
				_connectedStyles.Remove(style);
			}
		}

		foreach (StyleBox style in needed)
		{
			if (_connectedStyles.Add(style))
			{
				style.Changed += OnStyleBoxChanged;
				_connectedStyleList.Add(style);
			}
		}
	}

	private void DisconnectStyleSignals()
	{
		foreach (StyleBox style in _connectedStyleList)
		{
			if (IsInstanceValid(style))
			{
				style.Changed -= OnStyleBoxChanged;
			}
		}

		_connectedStyleList.Clear();
		_connectedStyles.Clear();
	}

	private void OnStyleBoxChanged()
	{
		if (!IsInsideTree())
		{
			return;
		}

		InvalidateModulatedStyles();
		Relayout();
		QueueRedraw();
	}

	public override void _Draw()
	{
		foreach (DocPanel panel in _panels)
		{
			GetDrawPanelStyle(panel).Draw(GetCanvasItem(), panel.Rect);
		}
	}

	public override Vector2 _GetMinimumSize()
	{
		return new Vector2(0.0f, _contentHeight);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			Relayout();
		}
		else if (what == NotificationThemeChanged)
		{
			_preferredTextWidths.Clear();
			Callable.From(RelayoutDeferred).CallDeferred();
		}
		else if (what == NotificationEnterTree)
		{
			_inLayout = false;

			if (IsNodeReady())
			{
				Callable.From(RebuildDeferred).CallDeferred();
			}
		}
		else if (what == NotificationExitTree)
		{
			_inLayout = false;
		}
		else if (what == NotificationVisibilityChanged)
		{
			if (IsVisibleInTree())
			{
				Callable.From(RelayoutDeferred).CallDeferred();
			}
		}
	}
}

