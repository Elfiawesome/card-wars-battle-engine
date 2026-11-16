extends MarginContainer

func set_data(data: Dictionary) -> void:
	$MarginContainer/VBoxContainer/Label.text = data.get("text", "")
	$MarginContainer/VBoxContainer/TextureRect.texture = data.get("texture", null)
