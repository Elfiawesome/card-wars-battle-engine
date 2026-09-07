using System;
using System.Collections.Generic;
using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scripts.core.registry;

public class SceneRegistry<TId> : Registry<TId, PackedScene> where TId : notnull;