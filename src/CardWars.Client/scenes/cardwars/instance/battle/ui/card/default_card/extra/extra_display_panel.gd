extends MarginContainer

@export var grid_container: Container
@export var icon_texture: TextureRect
@export var title_label: Label
@export var content_label: Label

func _ready() -> void:
	pass
	# Example
	# grid_container.set_items([
	# 	{},{"text":"5","filled":true},{},
	# 	{"text":"5","filled":true},{"crossed":true,"filled":true},{"text":"5","filled":true},
	# 	{},{"text":"5","filled":true},{}
	# ])

func set_grid(items: Array, grid_width: int = 3) -> void:
	if items.size() > 0:
		grid_container.visible = true
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
