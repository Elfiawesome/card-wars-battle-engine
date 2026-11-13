extends Control

var EMPTY_TEXTURE: Texture2D = load("res://assets/cardwars/texture/grid/grid_tile_empty.svg")
var FILLED_TEXTURE: Texture2D = load("res://assets/cardwars/texture/grid/grid_tile_filled.svg")


func set_data(data: Dictionary) -> void:
	modulate = data.get("color", Color.WHITE)
	
	var filled: bool = data.get("filled", false)
	if filled: $NinePatchRect.texture = FILLED_TEXTURE
	else: $NinePatchRect.texture = EMPTY_TEXTURE
	
	if data.has("text"):
		var text: String = data.get("text", "")
		$Label.visible = true
		$Label.text = text
	else:
		$Label.visible = false
	
	if data.has("crossed"):
		$XTexture.visible = data.get("crossed", false)
