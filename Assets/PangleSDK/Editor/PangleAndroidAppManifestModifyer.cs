using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;

static public class PangleAndroidAppManifestModifyer
{
    static public void Modify(AndroidManifest androidManifest)
    {
        //权限
        androidManifest.AddPermission("android.permission.INTERNET");
        androidManifest.AddPermission("android.permission.ACCESS_NETWORK_STATE");
        androidManifest.AddPermission("android.permission.WAKE_LOCK");

        //TTMultiProvider
        XmlElement provider2 = androidManifest.AddProvider("com.bytedance.sdk.openadsdk.multipro.TTMultiProvider");
        androidManifest.SetProviderAttribute(provider2, "authorities", "${applicationId}.TTMultiProvider");
        androidManifest.SetProviderAttribute(provider2, "exported", "false");
    }
}

public class AndroidAppManifestModifyer : IPostGenerateGradleAndroidProject
{
    public void OnPostGenerateGradleAndroidProject(string basePath)
    {
        AndroidManifest androidManifest = new AndroidManifest(GetManifestPath(basePath));

        PangleAndroidAppManifestModifyer.Modify(androidManifest);
        //...

        androidManifest.Save();
    }

    public int callbackOrder
    {
        get { return 1; }
    }

    private string _manifestFilePath;

    private string GetManifestPath(string basePath)
    {
        if (string.IsNullOrEmpty(_manifestFilePath))
        {
            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();
        }
        return _manifestFilePath;
    }
}

public class AndroidXmlDocument : XmlDocument
{
    private string path;
    protected XmlNamespaceManager nsMgr;
    public const string kAndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

    public AndroidXmlDocument(string path)
    {
        this.path = path;
        using (var reader = new XmlTextReader(this.path))
        {
            reader.Read();
            base.Load(reader);
        }
        nsMgr = new XmlNamespaceManager(NameTable);
        nsMgr.AddNamespace("android", kAndroidXmlNamespace);
    }

    public string Save()
    {
        return SaveAs(path);
    }

    public string SaveAs(string path)
    {
        using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
        {
            writer.Formatting = Formatting.Indented;
            Save(writer);
        }
        return path;
    }
}

public class AndroidManifest : AndroidXmlDocument
{
    private readonly XmlNode manifestNode;
    private readonly XmlElement applicationElement;
    private readonly XmlElement mainActivityElement;

    public AndroidManifest(string path) : base(path)
    {
        manifestNode = SelectSingleNode("/manifest");
        applicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        mainActivityElement = SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nsMgr) as XmlElement;
    }

    private XmlAttribute CreateAndroidAttribute(string key, string value)
    {
        XmlAttribute attr = CreateAttribute("android", key, kAndroidXmlNamespace);
        attr.Value = value;
        return attr;
    }

    public XmlElement AddPermission(string permissionName)
    {
        XmlElement child = CreateElement("uses-permission");
        child.Attributes.Append(CreateAndroidAttribute("name", permissionName));
        manifestNode.AppendChild(child);
        return child;
    }
    
    public XmlElement AddProvider(string providerName)
    {
        XmlElement child = CreateElement("provider");
        child.Attributes.Append(CreateAndroidAttribute("name", providerName));
        applicationElement.AppendChild(child);
        return child;
    }

    public void SetProviderAttribute(XmlElement provider, string key, string value)
    {
        provider.Attributes.Append(CreateAndroidAttribute(key, value));
    }

    public XmlElement AddProviderMetaData(XmlElement provider, string metaDataName)
    {
        XmlElement child = CreateElement("meta-data");
        child.Attributes.Append(CreateAndroidAttribute("name", metaDataName));
        provider.AppendChild(child);
        return child;
    }

    public void SetProviderMetaDataAttribute(XmlElement providerMetaData, string key, string value)
    {
        providerMetaData.Attributes.Append(CreateAndroidAttribute(key, value));
    }

    //例：SetApplicationAttribute("allowBackup", "true")
    public void SetApplicationAttribute(string key, string value)
    {
        applicationElement.Attributes.Append(CreateAndroidAttribute(key, value));
    }

    public void SetMainActivityName(string name)
    {
        mainActivityElement.Attributes.Append(CreateAndroidAttribute("name", name));
    }

    //例：SetMainActivityAttribute("hardwareAccelerated", "true")
    public void SetMainActivityAttribute(string key, string value)
    {
        mainActivityElement.Attributes.Append(CreateAndroidAttribute(key, value));
    }
}
