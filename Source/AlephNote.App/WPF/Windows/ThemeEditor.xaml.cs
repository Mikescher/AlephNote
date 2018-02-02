using ScintillaNET;
using System.Windows;
using System.Windows.Controls;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for ThemeEditor.xaml
	/// </summary>
	public partial class ThemeEditor : Window
	{
		private readonly ThemeEditorViewmodel viewmodel;

		public ThemeEditor()
		{
			InitializeComponent();
			DataContext = viewmodel = new ThemeEditorViewmodel(this);

			SourceEdit.TabWidth = 4;
			SourceEdit.Lexer = Lexer.Xml;
			SourceEdit.Styles[ScintillaNET.Style.Default].Font                   = "Courier New";

			SourceEdit.Styles[ScintillaNET.Style.Xml.Default].Bold               = true;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Default].Font               = "Courier New";
			SourceEdit.Styles[ScintillaNET.Style.Xml.Default].Size               = 10;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Default].SizeF              = 10F;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Default].Weight             = 700;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Tag].ForeColor              = System.Drawing.Color.Blue;
			SourceEdit.Styles[ScintillaNET.Style.Xml.TagUnknown].ForeColor       = System.Drawing.Color.Blue;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Attribute].ForeColor        = System.Drawing.Color.Red;
			SourceEdit.Styles[ScintillaNET.Style.Xml.AttributeUnknown].ForeColor = System.Drawing.Color.Red;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Number].ForeColor           = System.Drawing.Color.Red;
			SourceEdit.Styles[ScintillaNET.Style.Xml.DoubleString].Bold          = true;
			SourceEdit.Styles[ScintillaNET.Style.Xml.DoubleString].ForeColor     = System.Drawing.Color.BlueViolet;
			SourceEdit.Styles[ScintillaNET.Style.Xml.DoubleString].Weight        = 700;
			SourceEdit.Styles[ScintillaNET.Style.Xml.SingleString].Bold          = true;
			SourceEdit.Styles[ScintillaNET.Style.Xml.SingleString].ForeColor     = System.Drawing.Color.BlueViolet;
			SourceEdit.Styles[ScintillaNET.Style.Xml.SingleString].Weight        = 700;
			SourceEdit.Styles[ScintillaNET.Style.Xml.Comment].ForeColor          = System.Drawing.Color.Green;
			SourceEdit.Styles[ScintillaNET.Style.Xml.TagEnd].ForeColor           = System.Drawing.Color.Blue;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlStart].BackColor         = System.Drawing.Color.Yellow;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlStart].Bold              = true;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlStart].ForeColor         = System.Drawing.Color.Red;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlStart].Weight            = 700;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlEnd].BackColor           = System.Drawing.Color.Yellow;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlEnd].Bold                = true;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlEnd].ForeColor           = System.Drawing.Color.Red;
			SourceEdit.Styles[ScintillaNET.Style.Xml.XmlEnd].Weight              = 700;
			SourceEdit.Styles[ScintillaNET.Style.Xml.CData].ForeColor            = System.Drawing.Color.Orange;

		}

		private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var item = (sender as DataGrid)?.SelectedItem as ThemeEditorViewmodel.ThemeEditorDV;

			if (item == null) return;

			SourceEdit.InsertText(SourceEdit.CurrentPosition, $"\r\n\t\t<property name=\"{item.Key}\" value=\"{item.Default}\"/>");
		}
	}
}
