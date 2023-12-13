using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Map
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            List<List<String>> Crimes = GetCrimes();
            DrawCrimesTable(Crimes);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void AddCrimeTypeLabelClick(object sender, EventArgs e)
        {

        }

        private void AddCrimeAddButtonClick(object sender, EventArgs e)
        {
            var CrimeType = AddCrimeTypeInput.Text;
            var CrimeDate = AddCrimeDateInput.Value.ToString("dd.MM.yyyy"); ;
            var CrimeAddress = AddCrimeAddressInput.Text.ToLower();

            if (CrimeType == "" || CrimeAddress == "")
            {
                MessageBox.Show("Вид преступления и адрес преступления не могут быть пустыми!");
            } else
            {
                if (!CrimeAddress.Contains("кемерово"))
                {
                    CrimeAddress = "кемерово " + CrimeAddress;
                }

                HttpResponseMessage response = GetСoordinatesFromAddress(CrimeAddress);
                string AddressData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                List<List<String>> CorrectedAddressData = ConvertAddressData(AddressData);

                string CorrectAddress = GetAddressdDataParameter(CorrectedAddressData, "result");
                string AdressLatitude = GetAddressdDataParameter(CorrectedAddressData, "geo_lat");
                string AdressLongitude = GetAddressdDataParameter(CorrectedAddressData, "geo_lon");

                if (CorrectAddress != "г Кемерово" && CorrectAddress != "") {
                    List<List<String>> Crimes = GetCrimes();

                    List<List<String>> Data = new List<List<String>>();
                    List<String> DataLine = new List<String>();
                    int CrimeId = Crimes.Count + 1;
                    DataLine.Add($"Point ({AdressLongitude} {AdressLatitude})");
                    DataLine.Add(CrimeId.ToString());
                    DataLine.Add(CrimeDate);
                    DataLine.Add(CrimeType);
                    DataLine.Add(CorrectAddress);
                    Data.Add(DataLine);
                    WriteCrime(Data, true);

                    Crimes = GetCrimes();
                    DrawCrimesTable(Crimes);

                    MessageBox.Show("Преступление добавлено!");
                } else
                {
                    MessageBox.Show("Адрес не распознан!");
                }
            }

            Console.WriteLine(CrimeType, CrimeDate, CrimeAddress);
        }

        void WriteCrime(List<List<String>> Data, bool SaveOld)
        {
            using (var w = new StreamWriter("crimes.csv", SaveOld))
            {
                for (int RowIndex = 0; RowIndex < Data.Count; RowIndex++)
                {
                    string line = string.Format("{0};{1};{2};{3};{4}", Data[RowIndex][0], Data[RowIndex][1], Data[RowIndex][2], Data[RowIndex][3], Data[RowIndex][4]);
                    w.WriteLine(line);
                }
            }
        }

        string GetAddressdDataParameter(List<List<String>> Data, string NeedKey)
        {
            for (int RowIndex = 0; RowIndex < Data.Count; RowIndex++)
            {
                var Key = Data[RowIndex][0];
                var Value = Data[RowIndex][1];

                if (Key == NeedKey)
                {
                    return Value;
                }
            }

            return "";
        }

        List<List<String>> ConvertAddressData (string Data)
        {
            Data = Data.Substring(2, Data.Length - 4);
            string[] Pieces = Data.Split(',');

            string PieceKey = "";
            string PieceValue = "";

            List<List<String>> NeedData = new List<List<string>>();
            foreach (string Piece in Pieces)
            {
                string NeedPiece = Piece.Replace("\"", "");
                string[] StringPieces = NeedPiece.Split(':');
                if (StringPieces.Length == 2)
                {
                    if (PieceKey != "" && PieceValue != "")
                    {
                        List<String> DataRow = new List<String>();
                        DataRow.Add(PieceKey);
                        DataRow.Add(PieceValue);
                        NeedData.Add(DataRow);

                        PieceKey = "";
                        PieceValue = "";
                    }

                    PieceKey = StringPieces[0];
                    PieceValue = StringPieces[1];
                } else
                {
                    PieceValue += "," + StringPieces[0];
                }
            }

            return NeedData;
        }

        HttpResponseMessage GetСoordinatesFromAddress(String CrimeAddress) {
            List<Double> Сoordinates = new List<Double>();

            String API_SERVER = "https://cleaner.dadata.ru/api/v1/clean/address";
            String API_TOKEN = "a0be7f897fd08629f5acb2ed155aa6240befa5b2";
            String API_SERCRET = "191fbba6cc7ede033507429975aca958fa8af629";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Token " + API_TOKEN);
                client.DefaultRequestHeaders.Add("X-Secret", API_SERCRET);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = $"[ \"{CrimeAddress}\" ]";

                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                return client.PostAsync(API_SERVER, content).GetAwaiter().GetResult(); ;
            }
        }

        List<List<String>> GetCrimes()
        {
            var Data = new List<List<String>>();

            using (TextFieldParser parser = new TextFieldParser("crimes.csv", Encoding.UTF8))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    var Row = new List<String>();

                    foreach (string field in fields)
                    {
                        Row.Add(field);
                    }

                    Data.Add(Row);
                }
            }

            return Data;
        }

        List<List<String>> CutCrimesData(List<List<String>> Crimes, List<int> CutColumnsIndexes)
        {
            List<List<String>> CutedCrimesData = new List<List<String>>();

            for (int RowIndex = 0; RowIndex < Crimes.Count; RowIndex++)
            {
                List<String> Row = new List<String>();

                for (int ColumnIndex = 0; ColumnIndex < Crimes[RowIndex].Count; ColumnIndex++)
                {
                    if (!CutColumnsIndexes.Contains(ColumnIndex)) {
                        Row.Add(Crimes[RowIndex][ColumnIndex]);
                    }
                }

                CutedCrimesData.Add(Row); 
            }

            return CutedCrimesData;
        }

        void DrawCrimesTable(List<List<String>> Crimes)
        {
            List<int> CutColumnsIndexes = new List<int>();
            CutColumnsIndexes.Add(0);
            CutColumnsIndexes.Add(1);
            List<List<String>> CutedCrimes = CutCrimesData(Crimes, CutColumnsIndexes);

            ClearCrimesTable();
            if (CutedCrimes.Count > 0)
            {
                CrimesTable.ColumnCount = CutedCrimes[0].Count;
            }

            for (int RowIndex = 0; RowIndex < CutedCrimes.Count; RowIndex++)
            {
                CrimesTable.Rows.Add();


                for (int ColumnIndex = 0; ColumnIndex < CutedCrimes[RowIndex].Count; ColumnIndex++)
                {
                    CrimesTable.Rows[RowIndex].Cells[ColumnIndex].Value = CutedCrimes[RowIndex][ColumnIndex];
                }
            }
        }

        void ClearCrimesTable()
        {
            CrimesTable.Rows.Clear();
        }

        private void AddCrimeTypeInput_KeyPress(object sender, KeyPressEventArgs e)
        {
                e.Handled = true;
        }
    }
}
