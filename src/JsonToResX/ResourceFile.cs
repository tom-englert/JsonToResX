using System.Xml.Linq;

namespace JsonToResX;

public record ResourceNode(string Key, string? Text, string? Comment);

/// <summary>
/// Represents a set of localized resources.
/// </summary>
public class ResourceFile
{
    private const string Quote = "\"";
    private static readonly XName SpaceAttributeName = XNamespace.Xml.GetName("space");
    private static readonly XName TypeAttributeName = XNamespace.None.GetName("type");
    private static readonly XName MimetypeAttributeName = XNamespace.None.GetName("mimetype");
    private static readonly XName NameAttributeName = XNamespace.None.GetName("name");

    private const string EmptyResXTemplate =
"""
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
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
</root>
""";

    private readonly XDocument _document;

    private XElement DocumentRoot => _document.Root ?? throw new InvalidOperationException("Invalid resource file: No root node.");

    private IDictionary<string, Node> _nodes = new Dictionary<string, Node>();

    private readonly XName _dataNodeName;
    private readonly XName _valueNodeName;
    private readonly XName _commentNodeName;

    public ResourceFile(XDocument document)
    {
        _document = document;

        var defaultNamespace = DocumentRoot.GetDefaultNamespace();

        _dataNodeName = defaultNamespace.GetName("data");
        _valueNodeName = defaultNamespace.GetName("value");
        _commentNodeName = defaultNamespace.GetName("comment");

        UpdateNodes();
    }

    public ResourceFile(FileInfo file) : this(XDocument.Load(file.FullName))
    {
    }

    public ResourceFile() : this(XDocument.Parse(EmptyResXTemplate))
    {
    }

    public ICollection<ResourceNode> GetNodes()
    {
        return _nodes.Values
            .Select(node => new ResourceNode(node.Key, node.Text, node.Comment))
            .ToList()
            .AsReadOnly();
    }

    public string? GetValue(string key)
    {
        return !_nodes.TryGetValue(key, out var node) ? null : node.Text;
    }

    public void SetValue(string key, string? value)
    {
        if (!_nodes.TryGetValue(key, out var node))
        {
            node = CreateNode(key);
        }

        node.Text = value;
    }

    /// <summary>
    /// Saves this instance to the resource file.
    /// </summary>
    public void Save(string fileName)
    {
        _document.Save(fileName, SaveOptions.OmitDuplicateNamespaces);
    }

    private Node CreateNode(string key)
    {
        var content = new XElement(_valueNodeName);
        content.Add(new XText(string.Empty));

        var entry = new XElement(_dataNodeName, new XAttribute(NameAttributeName, key), new XAttribute(SpaceAttributeName, "preserve"));
        entry.Add(content);

        DocumentRoot.Add(entry);

        UpdateNodes();

        return _nodes[key];
    }

    private void UpdateNodes()
    {
        var data = DocumentRoot.Elements(_dataNodeName);

        var elements = data
            .Where(IsStringType)
            .Select(item => new Node(this, item))
            .ToArray();

        if (elements.Any(item => string.IsNullOrEmpty(item.Key)))
            throw new InvalidOperationException("Invalid resource file: Empty keys");

        try
        {
            _nodes = elements.ToDictionary(item => item.Key);
        }
        catch (ArgumentException ex)
        {
            var duplicateKeys = string.Join(", ", elements.GroupBy(item => item.Key).Where(group => group.Count() > 1).Select(group => Quote + group.Key + Quote));
            throw new InvalidOperationException($"Duplicate keys: {duplicateKeys}", ex);
        }
    }

    private static bool IsStringType(XElement entry)
    {
        var typeAttribute = entry.Attribute(TypeAttributeName);

        if (typeAttribute != null)
        {
            return string.IsNullOrEmpty(typeAttribute.Value) || typeAttribute.Value.StartsWith(nameof(String), StringComparison.OrdinalIgnoreCase);
        }

        var mimeTypeAttribute = entry.Attribute(MimetypeAttributeName);

        return mimeTypeAttribute == null;
    }

    private sealed class Node
    {
        private readonly ResourceFile _owner;

        private string? _text;
        private string? _comment;
        private readonly XElement _valueElement;

        public Node(ResourceFile owner, XElement element)
        {
            Element = element;
            _owner = owner;
            _valueElement = element.Element(_owner._valueNodeName) ?? throw new InvalidOperationException("Invalid resource file: No value element.");
            var nameAttribute = GetNameAttribute(element);
            Key = nameAttribute.Value;
            _text = LoadText();
        }

        private XElement Element { get; }

        public string Key { get; }

        public string? Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;

                if (_valueElement.FirstNode == null)
                {
                    _valueElement.Add(value);
                }
                else
                {
                    _valueElement.FirstNode.ReplaceWith(value);
                }
            }
        }

        public string Comment => _comment ??= LoadComment();

        private string LoadText()
        {
            return _valueElement.FirstNode is XText textNode ? textNode.Value : string.Empty;
        }

        private string LoadComment()
        {
            var commentElement = Element.Element(_owner._commentNodeName);

            return commentElement?.FirstNode is XText textNode ? textNode.Value : string.Empty;
        }

        private static XAttribute GetNameAttribute(XElement entry)
        {
            var nameAttribute = entry.Attribute(NameAttributeName);

            return nameAttribute ?? throw new InvalidOperationException("Invalid resource file: No name attribute");
        }
    }

    public override string ToString()
    {
        return _document.ToString(SaveOptions.OmitDuplicateNamespaces);
    }
}
