extends Node3D

@onready var ui: Control = $UI
@onready var hand_container: Control = $UI/Hand

func _ready() -> void:
	for i in range(5):
		add_card()
	arrange_hands()

func add_card() -> void:
	var card: Control = load("res://scenes/test/card.tscn").instantiate()
	hand_container.add_child(card)
	arrange_hands()

func arrange_hands() -> void:
	print("Calc")
	var cards = hand_container.get_children()
	var card_count = cards.size()
	
	if card_count == 0:
		return
	
	var midpos := Vector2(hand_container.size.x / 2, hand_container.size.y)
	var hand_spread_deg: float = 180.0
	var card_angle_spread: float = 60.0
	for i in range(card_count):
		var c: Node = cards[i]
		if c is Control:
			var angle_deg: float = + 90 + (180-hand_spread_deg)/2 + (hand_spread_deg/card_count) * i + (hand_spread_deg/card_count)/2
			var angle := deg_to_rad(angle_deg)
			print(rad_to_deg(angle))
			c.position.x = midpos.x + sin(angle) * 300
			c.position.y = midpos.y + cos(angle) * 100
			
			var c_angle_deg: float = +card_angle_spread/2 - (card_angle_spread/card_count) * i - (card_angle_spread/card_count)/2
			var c_angle := deg_to_rad(c_angle_deg)
			c.offset_transform_rotation = c_angle
