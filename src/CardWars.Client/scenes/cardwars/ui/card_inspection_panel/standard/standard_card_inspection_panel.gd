extends MarginContainer

@export var grid_container: Container
@export var icon_texture: TextureRect
@export var title_label: Label
@export var content_label: Label

# We can set from the consumer to set where the card is at so we can animate if needed
@export var card_dimensions: Rect2
@export var panel_index: int = 0
@export var animated_from_right: bool = false

func _ready() -> void:
	if card_dimensions.size == Vector2.ZERO:
		return

	var target := position
	var offset := 32.0 if animated_from_right else -32.0
	position = target + Vector2(offset, 0.0)
	modulate.a = 0.0

	var tween := create_tween()
	tween.set_parallel(true).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	var delay := 0.03 * panel_index
	tween.tween_property(self, "position", target, 0.25).set_delay(delay)
	tween.tween_property(self, "modulate:a", 1.0, 0.25).set_delay(delay)

	# Grid Example
	# grid_container.set_items([
	# 	{},{"text":"5","filled":true},{},
	# 	{"text":"5","filled":true},{"crossed":true,"filled":true},{"text":"5","filled":true},
	# 	{},{"text":"5","filled":true},{}
	# ])

func set_grid(items: Array, grid_width: int = 3) -> void:
	if items.size() > 0:
		grid_container.visible = true
		var columns: int = max(1, grid_width)
		var rows := int(ceil(float(items.size()) / float(columns)))
		grid_container.custom_minimum_size = Vector2(32.0 * columns, 32.0 * rows)
		if grid_container is DataContainer:
			grid_container.set_items(items)
		if grid_container is GridContainer:
			grid_container.columns = grid_width
	else:
		grid_container.visible = false


func set_content(title: String, texture: Texture2D, text: String) -> void:
	icon_texture.texture = texture
	title_label.text = title
	content_label.text = text
