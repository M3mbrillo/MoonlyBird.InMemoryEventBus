﻿namespace MoonlyBird.InMemoryEventBus.Abstract.Model;

public record Event<T>(T? Data, EventMetadata? Metadata = default);
