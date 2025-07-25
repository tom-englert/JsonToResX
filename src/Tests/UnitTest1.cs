using System.Xml.Linq;

using JsonToResX;

namespace Tests;

public class Tests
{
    private const string ResX =
        """
        <root>
          <xsd:schema id="root" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
            <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
            <xsd:element name="root" msdata:IsDataSet="true">
              <xsd:complexType>
                <xsd:choice maxOccurs="unbounded">
                  <xsd:element name="metadata">
                    <xsd:complexType>
                      <xsd:sequence>
                        <xsd:element name="value" type="xsd:string" minOccurs="0" />
                      </xsd:sequence>
                      <xsd:attribute name="name" use="required" type="xsd:string" />
                      <xsd:attribute name="type" type="xsd:string" />
                      <xsd:attribute name="mimetype" type="xsd:string" />
                      <xsd:attribute ref="xml:space" />
                    </xsd:complexType>
                  </xsd:element>
                  <xsd:element name="assembly">
                    <xsd:complexType>
                      <xsd:attribute name="alias" type="xsd:string" />
                      <xsd:attribute name="name" type="xsd:string" />
                    </xsd:complexType>
                  </xsd:element>
                  <xsd:element name="data">
                    <xsd:complexType>
                      <xsd:sequence>
                        <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                        <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
                      </xsd:sequence>
                      <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
                      <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
                      <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
                      <xsd:attribute ref="xml:space" />
                    </xsd:complexType>
                  </xsd:element>
                  <xsd:element name="resheader">
                    <xsd:complexType>
                      <xsd:sequence>
                        <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                      </xsd:sequence>
                      <xsd:attribute name="name" type="xsd:string" use="required" />
                    </xsd:complexType>
                  </xsd:element>
                </xsd:choice>
              </xsd:complexType>
            </xsd:element>
          </xsd:schema>
          <resheader name="resmimetype">
            <value>text/microsoft-resx</value>
          </resheader>
          <resheader name="version">
            <value>2.0</value>
          </resheader>
          <resheader name="reader">
            <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
          </resheader>
          <resheader name="writer">
            <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
          </resheader>
          <data name="app_title" xml:space="preserve">
            <value>My Application</value>
          </data>
          <data name="app_subtitle" xml:space="preserve">
            <value>Welcome to my application</value>
          </data>
          <data name="app_greeting" xml:space="preserve">
            <value>Hello ${name}</value>
          </data>
        </root>                
        """;

    private const string Json =
        """
        {
          "app.title": "My Application",
          "app.subtitle": "Welcome to my application",
          "app.greeting": "Hello {{name}}"
        }
        """;

    [Test]
    public void TestResXToJson()
    {
        var result = Infrastructure.ConvertToJson(new(XDocument.Parse(ResX)));

        Assert.That(Normalize(result), Is.EqualTo(Normalize(Json)));
    }

    [Test]
    public void TestJsonToResX()
    {
        var result = Infrastructure.ConvertToResX(Json, new());

        Assert.That(Normalize(result.ToString()), Is.EqualTo(Normalize(ResX)));
    }

    private static string Normalize(string value)
    {
        return value.Replace("\r\n", "\n").Trim();
    }
}
