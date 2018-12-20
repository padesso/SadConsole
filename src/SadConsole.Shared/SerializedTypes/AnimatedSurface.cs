﻿using FrameworkPoint = Microsoft.Xna.Framework.Point;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using SadConsole.Surfaces;

namespace SadConsole.SerializedTypes
{
    public class AnimatedSurfaceConverterJson : JsonConverter<Surfaces.AnimatedScreenObject>
    {
        public override void WriteJson(JsonWriter writer, AnimatedScreenObject value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, (AnimatedSurfaceSerialized)value);
        }

        public override AnimatedScreenObject ReadJson(JsonReader reader, Type objectType, AnimatedScreenObject existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return serializer.Deserialize<AnimatedSurfaceSerialized>(reader);
        }
    }

    [DataContract]
    public class AnimatedSurfaceSerialized : ScreenObjectSerialized
    {
        [DataMember] public BasicSurfaceSerialized[] Frames;
        [DataMember] public int Width;
        [DataMember] public int Height;
        [DataMember] public float AnimationDuration;
        [DataMember] public FontSerialized Font;
        [DataMember] public string Name;
        [DataMember] public bool Repeat;
        [DataMember] public FrameworkPoint Center;

        public static implicit operator AnimatedSurfaceSerialized(Surfaces.AnimatedScreenObject surface)
        {
            return new AnimatedSurfaceSerialized()
            {
                Frames = surface.Frames.Select(s => (BasicSurfaceSerialized) s).ToArray(),
                Width = surface.Width,
                Height = surface.Height,
                AnimationDuration = surface.AnimationDuration,
                Name = surface.Name,
                Font = surface.Font,
                Repeat = surface.Repeat,
                Center = surface.Center,
                Position = surface.Position,
                IsVisible = surface.IsVisible,
                IsPaused = surface.IsPaused
            };
        }

        public static implicit operator Surfaces.AnimatedScreenObject(AnimatedSurfaceSerialized serializedObject)
        {
            return new Surfaces.AnimatedScreenObject(serializedObject.Name, serializedObject.Width,
                                         serializedObject.Height, serializedObject.Font)
            {
                frames = new List<Surfaces.CellSurface>(serializedObject.Frames.Select(s => (Surfaces.CellSurface) s).ToArray()),
                CurrentFrameIndex = 0,
                AnimationDuration = serializedObject.AnimationDuration,
                Repeat = serializedObject.Repeat,
                Center = serializedObject.Center,
                Position = serializedObject.Position,
                IsVisible = serializedObject.IsVisible,
                IsPaused = serializedObject.IsPaused
            };
        }
    }
}
