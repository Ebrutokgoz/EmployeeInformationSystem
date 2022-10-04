using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace PersonelVeİletişimWindowsFormApp
{
    public partial class Main : Form
    {
        OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=‪C:\Users\ebrut\Desktop\personel.xls; Extended Properties='Excel 12.0 xml;HDR=YES;'");
        string selectedId;
        public Main()
        {
            InitializeComponent();
        }

        //Veri tablosuna tüm verileri listeler
        public void list() 
        {
            try
            { 
               OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [Sayfa1$]", connection);
               DataTable dt = new DataTable();
               da.Fill(dt);
               dataGridView1.DataSource = dt;
                datagridViewColumnsWidth();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //Veri tablosunun kolon boyutlarını ayarlar
        public void datagridViewColumnsWidth()
        {
            DataGridViewColumn column1 = dataGridView1.Columns[0];
            column1.Visible = false;
            DataGridViewColumn column2 = dataGridView1.Columns[1];
            column2.Width = 250;
            DataGridViewColumn column3 = dataGridView1.Columns[2];
            column3.Width = 100;
            DataGridViewColumn column4 = dataGridView1.Columns[3];
            column4.Width = 190;
            DataGridViewColumn column5 = dataGridView1.Columns[4];
            column5.Width = 200;
            DataGridViewColumn column6 = dataGridView1.Columns[5];
            column6.Width = 200;
            DataGridViewColumn column7 = dataGridView1.Columns[6];
            column7.Width = 200;
            DataGridViewColumn column8 = dataGridView1.Columns[7];
            column8.Width = 80;

        }

        //Combobox'a birim adlarını ekler
        public void addUnitItems()
        {
            connection.Open();
            OleDbDataAdapter da = new OleDbDataAdapter("SELECT DISTINCT Birim FROM [Sayfa1$]", connection); //Excel dosyasından sadece birim adlarını çeker
            DataTable dt = new DataTable();
            da.Fill(dt); //Birim adları data table'a eklenir
            int rows = dt.Rows.Count;  //Data table'daki satırları sayarak kaç birim adı olduğunu tutar
            unitComboBox.Items.Add("-");
            for (int i = 1; i < rows; i++)
            {
                unitComboBox.Items.Add(dt.Rows[i]["Birim"].ToString()); //Combobox'a birim adlarını teker teker ekler
            }
            connection.Close();
        }

        //Combobox boyutunu ayarlar
        public void comboboxWidth()
        {
            int width = unitComboBox.DropDownWidth;
            Graphics g = unitComboBox.CreateGraphics();
            Font font = unitComboBox.Font;
            foreach(string s in unitComboBox.Items) //Combobox'daki her birim adının boyutuna bakar en uzun olanı width değişkeninde tutar
            {
                int measuredWidth = (int)g.MeasureString(s, font).Width;
                if (width < measuredWidth)
                    width = measuredWidth;
            }
            unitComboBox.DropDownWidth = width; //Combobox'un enini width'e (en uzun birim isminin boyutuna) eşit olacak şekilde büyütür
        }

        //Kullanıcı arayüzü ekranda açılırken çalışacak olan metodları ekler
        private void Main_Load(object sender, EventArgs e)
        {
           list();
           addUnitItems();
           comboboxWidth();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT * FROM [Sayfa1$]"; //Sql sorgu cümlesinin giriş kısmı query'de tutulur
                DataTable dt = new DataTable();
                using (var command = new OleDbCommand())
                {
                    List<string> clauses = new List<string>(); //Boş bir string list oluşturulur

                    if(unitComboBox.SelectedItem != null) //Combobox'da bir seçim yapıldıysa true değer döndürür
                    {
                        if(unitComboBox.SelectedItem.ToString() != "-")
                        {
                            clauses.Add("Birim LIKE @p1"); //Boş stringe sql sorgusunun koşulları eklenir
                            command.Parameters.AddWithValue("@p1", unitComboBox.SelectedItem.ToString().Trim() + "%");
                        }
                    }

                    if (!string.IsNullOrEmpty(nameText.Text)) //Ad textbox'ına bir değer girildiyse true değeri döndürür
                    {
                        clauses.Add("Adı LIKE @p2");
                        command.Parameters.AddWithValue("@p2", nameText.Text.Trim() + "%");
                    }

                    if (!string.IsNullOrEmpty(surnameText.Text)) //Soyad textbox'ına bir değer girildiyse true değeri döndürür
                    {
                        clauses.Add("Soyadı LIKE @p3");
                        command.Parameters.AddWithValue("@p3", surnameText.Text.Trim() + "%");
                    }

                    if (!string.IsNullOrEmpty(roomText.Text)) //Oda No textbox'ına bir değer girildiyse true değeri döndürür
                    {
                        clauses.Add("OdaNo LIKE @p4");
                        command.Parameters.AddWithValue("@p4", roomText.Text.Trim() + "%");
                    }

                    if (!string.IsNullOrEmpty(phoneText.Text)) //Dahili textbox'ına bir değer girildiyse true değeri döndürür
                    {
                        clauses.Add("Dahili LIKE @p5");
                        command.Parameters.AddWithValue("@p5", phoneText.Text.Trim() + "%");
                    }

                    if (clauses.Count() > 0) //Başta boş olan clauses'e eklenen tüm koşul cümlelerei giriş cümlesinin tutulduğu query değişkenine eklenir sql sorgu cümlesi oluşturulur.
                    {
                        query += " WHERE " + string.Join(" AND ", clauses);
                    }
                    command.Connection = connection;
                    command.CommandText = query;  //Sql komutu, veritabanı ve sorgu cümlesi ile ilişkilendirilir.
                    connection.Open();

                    dt.Load(command.ExecuteReader());
                }
                dataGridView1.DataSource = dt; //Veri tablosunda listelenir
            }

            catch
            {
                MessageBox.Show("Lütfen yeniden deneyin.","Bir hata oluştu",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            connection.Close();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            try
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("UPDATE [Sayfa1$] SET Birim=@p1, Adı=@p2, Soyadı=@p3, OdaNo=@p4, Dahili=@p5 WHERE ID=@p6", connection); //Sql sorgu cümlesi oluşturulur
                //Kullanıcının girdiği veriler Textboxlardan ve comboboxtan çekilir
                command.Parameters.AddWithValue("@p1", unitComboBox.SelectedItem.ToString());
                command.Parameters.AddWithValue("@p2", nameText.Text);
                command.Parameters.AddWithValue("@p3", surnameText.Text);
                command.Parameters.AddWithValue("@p4", roomText.Text);
                command.Parameters.AddWithValue("@p5", phoneText.Text);
                command.Parameters.AddWithValue("@p6", selectedId);
                command.ExecuteNonQuery();

                //Güncellenen personelin bilgileri veri tablosunda gösterilir
                OleDbCommand command2 = new OleDbCommand("SELECT * FROM [Sayfa1$] WHERE ID=@p6", connection);
                command2.Parameters.AddWithValue("@p6", selectedId);
                OleDbDataAdapter da = new OleDbDataAdapter(command2);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }

            catch(Exception ex) //Hata varsa yakalar ve uyarı verir
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("Güncelleme yapmak istediğiniz veriyi tablodan seçin \nve tüm alanları doldurduğunuzdan emin olun!","Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            datagridViewColumnsWidth();
            connection.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            unitComboBox.SelectedItem = null;
            try
            {
                //Veri tablosunda çift tıklanan satırdan bilgileri çeker textboxlara yazdırır
                int selected = dataGridView1.SelectedCells[0].RowIndex;
                selectedId = dataGridView1.Rows[selected].Cells[0].Value.ToString();
                nameText.Text = dataGridView1.Rows[selected].Cells[4].Value.ToString();
                surnameText.Text = dataGridView1.Rows[selected].Cells[5].Value.ToString();
                roomText.Text = dataGridView1.Rows[selected].Cells[2].Value.ToString();
                phoneText.Text = dataGridView1.Rows[selected].Cells[7].Value.ToString();
                string unit = dataGridView1.Rows[selected].Cells[1].Value.ToString();
                int i = 0;
                foreach (string item in unitComboBox.Items) //Comboboxda personel bilgisinde yer alan birimi gösterir
                {
                    if (unit == item)
                        break;
                    i++;
                }
                unitComboBox.SelectedItem = unitComboBox.Items[i];
            }
            catch
            {
                MessageBox.Show("Güncelleme yapabilmek için bilgi içeren bir satır seçmelisiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        //Texbox'daki verileri ve combobox'da seçili veriyi temizler.
        private void clearButton_Click(object sender, EventArgs e)
        {
            unitComboBox.SelectedItem = null;
            nameText.Clear();
            surnameText.Clear();
            roomText.Clear();
            phoneText.Clear();

            list();
        }

    }
}