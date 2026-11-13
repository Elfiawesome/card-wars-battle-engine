extends MarginContainer

func set_data(data: Dictionary) -> void:
	$VBoxContainer/Label.text = data["text"]
	$VBoxContainer/TextureRect.texture = data["texture"]
