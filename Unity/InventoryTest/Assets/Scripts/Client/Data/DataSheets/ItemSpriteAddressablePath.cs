

using Cathei.BakingSheet.Unity;

public class ItemSpriteAddressablePath : AddressablePath
{
    public override string BasePath => "Assets/UI/Images/UISprite/Item/";
    public override string Extension => "png";

    public ItemSpriteAddressablePath(string rawValue) : base(rawValue) { }
}