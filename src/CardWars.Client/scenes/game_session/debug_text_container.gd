extends HBoxContainer

@export var text: String:
	set(value):
		$Text.text = value
		text= value
@export var content: String:
	set(value):
		$Content.text = value
		content = value
