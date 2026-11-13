extends HBoxContainer

@onready var color_rect: ColorRect = $AbilityTriggerContainer/ColorRect
@onready var texture_rect: TextureRect = $AbilityTriggerContainer/HBoxContainer/Control/MarginContainer/TextureRect
@onready var label: Label = $AbilityTriggerContainer/HBoxContainer/MarginContainer/Label
@onready var ability_description: Label = $AbilityDescription

func set_data(data: Dictionary = {}) -> void:
	var color_value: Variant = data.get("color", Color.RED)
	if color_value is Color:
		color_rect.color = color_value

	var texture_value: Variant = data.get("texture")
	if texture_value is Texture2D:
		texture_rect.texture = texture_value

	label.text = str(data.get("ability_type", "ERROR"))
	ability_description.text = str(data.get("description", "DESCRIPTION ERROR"))
