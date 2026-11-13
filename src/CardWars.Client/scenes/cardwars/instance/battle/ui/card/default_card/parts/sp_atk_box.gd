extends MarginContainer

func set_data(data: Dictionary) -> void:
	$VBoxContainer/Label.text = data.get("text", "")
	$VBoxContainer/TextureRect.texture = data.get("texture", null)
