namespace MapDrawer;

public static class TileResources
{
    public static string GetResource(Style style, bool dark = false, bool dangerous = false)
    {
        var name = style switch
        {
            Style.Common => "mine",
            Style.Frost => "mine_frost",
            Style.Lava => "mine_lava",
            Style.Desert => "mine_desert",
            Style.Slime => "mine_slime",
            Style.Dinosaur => "mine_dino",
            Style.Quarry => "mine_quarryshaft",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Invalid style")
        };

        if (dark && style is not (Style.Slime or Style.Dinosaur or Style.Quarry))
            name += "_dark";

        if (dangerous && style is not (Style.Dinosaur or Style.Quarry))
            name += "_dangerous";

        return name;
    }

    public enum Style
    {
        Common,
        Frost,
        Lava,
        Desert,
        Slime,
        Dinosaur,
        Quarry
    }
}