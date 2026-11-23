extends MarginContainer

@export var grid_container: Container
@export var icon_texture: TextureRect
@export var title_label: Label
@export var content_label: Label

var card_rect: Rect2

func _ready() -> void:
	pass

func play_intro(delay: float = 0) -> void:
	var duration: float = 0.25
	var t: Tween = create_tween()
	t.set_parallel(true).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	t.tween_property(self, "modulate:a", modulate.a, duration).set_delay(delay)
	t.tween_property(self, "offset_transform_position", Vector2.ZERO, duration).set_delay(delay)

	z_index -= 1
	modulate.a = 0.0
	offset_transform_position.x = (card_rect.position - position).x

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


func set_content(title: String, texture: Texture2D, text: String, color: Color) -> void:
	icon_texture.texture = texture
	title_label.text = title
	content_label.text = text
	
	$VBoxContainer/Top.color = color
	$VBoxContainer/MarginContainer/MarginContainer/VBoxContainer/HBoxContainer/TextureRect.modulate = color
