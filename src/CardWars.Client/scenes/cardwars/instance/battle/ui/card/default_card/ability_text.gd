extends HBoxContainer

@onready var color_rect: ColorRect = $AbilityTriggerContainer/ColorRect
@onready var texture_rect: TextureRect = $AbilityTriggerContainer/HBoxContainer/Control/MarginContainer/TextureRect
@onready var label: Label = $AbilityTriggerContainer/HBoxContainer/MarginContainer/Label
@onready var ability_description: Label = $AbilityDescription

func set_data(
	description: String = "DESCRIPTION ERROR",
	ability_trigger: String = "ERROR",
	ability_color: Color = Color.RED,
	ability_texture: Texture2D = null
) -> void:
	color_rect.color = ability_color
	texture_rect.texture = ability_texture
	label.text = ability_trigger
	ability_description.text = description
	print(texture_rect.texture)
