// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Skinning;

namespace osu.Game.Database
{
    public class LegacySkinExporter : LegacyExporter<SkinInfo>
    {
        protected override string FileExtension => ".osk";

        public LegacySkinExporter(Storage storage, INotificationOverlay? notificationOverlay)
            : base(storage, notificationOverlay)
        {
        }
    }
}
