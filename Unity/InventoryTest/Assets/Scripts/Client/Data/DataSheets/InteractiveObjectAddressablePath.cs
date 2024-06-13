using Cathei.BakingSheet.Unity;

public class InteractiveObjectAddressablePath : AddressablePath
{
    public override string BasePath => "Assets/UI/Images/UISprite/InteractiveObject/";
    public override string Extension => "png";

    public InteractiveObjectAddressablePath(string rawValue) : base(rawValue) { }
}