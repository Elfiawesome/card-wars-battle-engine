extends Control

@onready var grid_container: DataContainer = $VBoxContainer/Content/MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/MarginContainer/GridContainer
@onready var texture_rect: TextureRect = $VBoxContainer/Content/MarginContainer/VBoxContainer/HBoxContainer/TextureRect
@onready var title_label: Label = $VBoxContainer/Content/MarginContainer/VBoxContainer/HBoxContainer/TitleLabel
@onready var content_label: Label = $VBoxContainer/Content/MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/ContentLabel

func _ready() -> void:
	pass
	# Example
	# grid_container.set_items([
	# 	{},{"text":"5","filled":true},{},
	# 	{"text":"5","filled":true},{"crossed":true,"filled":true},{"text":"5","filled":true},
	# 	{},{"text":"5","filled":true},{}
	# ])

func set_grid(items: Array) -> void:
	if items.size() > 0:
		grid_container.visible = true
		grid_container.set_items(items)
	else:
		grid_container.visible = false


func set_content(title: String, texture: Texture2D, text: String) -> void:
	texture_rect.texture = texture
	title_label.text = title
	content_label.text = text