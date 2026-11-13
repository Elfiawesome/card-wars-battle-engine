extends Control

@onready var grid_container: DataContainer = $VBoxContainer/Content/MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/MarginContainer/GridContainer

func _ready() -> void:
	grid_container.set_items([
		{},{"text":"5","filled":true},{},
		{"text":"5","filled":true},{"crossed":true,"filled":true},{"text":"5","filled":true},
		{},{"text":"5","filled":true},{}
	])
