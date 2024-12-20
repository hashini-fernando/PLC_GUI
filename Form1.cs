using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Text.Json;
using Sharp7;


namespace PLC_GUI
{
    public partial class Form1 : Form
    {
        private TextBox txtPlcIp, txtStatus, txtDBServer, txtDBName, txtDBUserID, txtDBPassword, txtDBTableName, txtDBTableRowno, txtDBTablecolumnno, txtPlcdb, txtbyteoffset, txtbitoffset ;
        private ComboBox DataType;
        private List<DataMapping> mappings;

        private PlcCommunicator _plcCommunicator;

        private DatabaseCommunicator _dbCommunicator;
        
        private Button plcbtnConnect, btnAddMapping, btnSaveMappings, btnLoadMappings ,btnRunMappings;
        private System.Threading.CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            mappings = new List<DataMapping>();
            _plcCommunicator = new PlcCommunicator();
            // InitializeDatabaseCommunicator();
            // Trigger connection logic on form load
            this.Load += Form1_Load;
   
        }

          private void Form1_Load(object sender, EventArgs e)
    {
        // Attempt to connect automatically on startup
        Button_Click(plcbtnConnect, EventArgs.Empty);
    }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 900);
            this.Text = "PLC SQL Application";

            // PLC IP Address
            AddLabel("PLC IP Address", 20, 20);
            txtPlcIp = AddTextBox(140, 20);

            // Database Server
            AddLabel("Database Server", 20, 60);
            txtDBServer = AddTextBox(140, 60);

            // Database Name
            AddLabel("Database Name", 20, 100);
            txtDBName = AddTextBox(140, 100);

            // Database User ID
            AddLabel("MySQL UserID", 20, 140);
            txtDBUserID = AddTextBox(140, 140);

            // Database Password
            AddLabel("MySQL Password", 10, 180);
            txtDBPassword = AddTextBox(140, 180, true);

            // PLC Connect Button
            plcbtnConnect = AddButton("Connect", 140, 220);
            plcbtnConnect.Click += Button_Click;


            var writePLCButton = AddButton("Write to PLC", 140, 260);
            writePLCButton.Click += btnWritePLC_Click;



            // Status TextBox
            txtStatus = AddTextBox(200, 440, isMultiline: true);
            txtStatus.Size = new System.Drawing.Size(600, 400);
            txtStatus.ScrollBars = ScrollBars.Vertical;


            // Database Table Name
            AddLabel("Table Name", 500, 20);
            txtDBTableName = AddTextBox(620, 20);

            // Database Row Id
            AddLabel("Row Id", 500, 60);
            txtDBTableRowno = AddTextBox(620, 60);

            // Database Table Column Name
            AddLabel("Table Column", 500, 100);
            txtDBTablecolumnno = AddTextBox(620, 100);

            // PLC DB Number
            AddLabel("PLC DB Number", 500, 140);
            txtPlcdb = AddTextBox(620, 140);

            // Byte Offset
            AddLabel("Byte Offset", 500, 180);
            txtbyteoffset = AddTextBox(620, 180);

            // Bit Offset
            AddLabel("Bit Offset", 500, 220);
            txtbitoffset = AddTextBox(620, 220);

            // Data Type
            AddLabel("Data Type", 500, 260);
            DataType = AddComboBox(620, 260, new[] { "string", "int", "bool", "dint", "time", "sint", "uint", "word", "dword", "real", "lreal" });

            // Add Mapping Button
            btnAddMapping = AddButton("Add Mapping", 160, 360);
            btnAddMapping.Click += btnAddMapping_Click;

            // Save Mappings Button
            btnSaveMappings = AddButton("Save Mappings", 380, 360);
            btnSaveMappings.Click += btnSaveMappings_Click;

            // Load Mappings Button
            btnLoadMappings = AddButton("Load Mappings", 600, 360);
            btnLoadMappings.Click += btnLoadMappings_Click;

            //run buutton
            btnRunMappings = AddButton("Run ", 380, 400);
            btnRunMappings.Click += Run_Mappings;
        }

        private Label AddLabel(string text, int x, int y)
        {
            Label label = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
            this.Controls.Add(label);
            return label;
        }

        private TextBox AddTextBox(int x, int y, bool isPassword = false, bool isMultiline = false)
        {
            TextBox textBox = new TextBox
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(200, 20),
                PasswordChar = isPassword ? '*' : '\0',
                Multiline = isMultiline
            };
            this.Controls.Add(textBox);
            return textBox;
        }

        private Button AddButton(string text, int x, int y)
        {
            Button button = new Button
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(200, 30)
            };
            this.Controls.Add(button);
            return button;
        }

        private ComboBox AddComboBox(int x, int y, string[] items)
        {
            ComboBox comboBox = new ComboBox
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(200, 20)
            };
            comboBox.Items.AddRange(items);
            this.Controls.Add(comboBox);
            return comboBox;
        }

        
        private (string plcIp, string server, string database, string userId, string password) GetConnectionDetails()
        {

             string plcIp = txtPlcIp.Text.Trim();
            string server = txtDBServer.Text.Trim();
            string database = txtDBName.Text.Trim();
            string userId = txtDBUserID.Text.Trim();
            string password = txtDBPassword.Text.Trim();

            return (plcIp ,server, database, userId, password);
        }




        private async void Button_Click(object sender, EventArgs e)
        {
           
        var (plcIp, server, database, userId, password) = GetConnectionDetails();

            if (string.IsNullOrEmpty(plcIp))
            {
                txtStatus.Text = "Please enter a valid IP address.";
                return;
            }

            
            try
            {
                // Connect to PLC
                
                if (!_plcCommunicator.Connect(plcIp, 0, 1))
                {
                    txtStatus.Text = "Failed to connect to the PLC.";
                    
                    return;
                }
                txtStatus.Text = $"Connected to PLC at {plcIp}";

                 _dbCommunicator = new DatabaseCommunicator(server, database, userId, password);

                if(!await _dbCommunicator.TestConnectionAsync()){
                    txtStatus.Text = "Failed to connect to dbserver.";
                    return;
                }
                txtStatus.Text =  $"Connected to database server";

            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                 _plcCommunicator.Disconnect();
            }
        }

                private async void Run_Mappings(object sender, EventArgs e)
            {
                if (mappings == null || mappings.Count == 0)
                {
                    txtStatus.Text = "No mappings loaded. Please load mappings before running.";
                    return;
                }

                string plcIp = txtPlcIp.Text.Trim();
              

                if (string.IsNullOrEmpty(plcIp))
                {
                    txtStatus.Text = "Please enter a valid IP address.";
                    return;
                }
                try
                {
                    
                    if (!_plcCommunicator.Connect(plcIp, 0, 1))
                    {
                        txtStatus.Text = "Failed to connect to the PLC.";
                        return;
                    }
                    txtStatus.Text = $"Connected to PLC at {plcIp}";

                    

                    cancellationTokenSource = new System.Threading.CancellationTokenSource();
                    var token = cancellationTokenSource.Token;
                    await PollPlcAndUpdateDatabaseAsync (token);

                }
                catch (Exception ex)
                {
                    txtStatus.Text = $"Error: {ex.Message}";
                }
                finally
                {
                    _plcCommunicator.Disconnect();
                }
            }



        private async Task PollPlcAndUpdateDatabaseAsync(  System.Threading.CancellationToken token)
        {
                 if(!await _dbCommunicator.TestConnectionAsync()){
                    txtStatus.Text = "Failed to connect to the PLC.";
                    return;
                }
                txtStatus.Text =  $"Connected to database server";
            
                
                

                // Start WebSocket server
                var webSocketManager = new WebSocketManager(8080); 
                webSocketManager.Start();

                while (!token.IsCancellationRequested)
                {
                    foreach (var mapping in mappings)
                    {
                        try
                        {
                            // Read data from PLC asynchronously
                            object value = await Task.Run(() => _plcCommunicator.ReadPLCData(mapping));

                            // Update the database with the new value
                            await _dbCommunicator.UpdateDatabaseAsync( mapping.TableName, mapping.ColumnName, mapping.RowId, value);

                            // Send WebSocket update
                            string updateMessage = $"Updated {mapping.TableName}.{mapping.ColumnName} (row ID {mapping.RowId}) with value: {value}";
                            webSocketManager.BroadcastUpdate(updateMessage); // Send update to all connected clients

                            // Update the status textbox with the latest value
                            txtStatus.Text = updateMessage;

                            // To avoid flooding UI thread, update UI every few iterations
                            if (mappings.IndexOf(mapping) % 5 == 0)
                            {
                                Invoke(new Action(() =>
                                {
                                    txtStatus.Text += "\nUpdating values...";
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            txtStatus.Text += $"\nError reading or updating data for {mapping.MappingName}: {ex.Message}";
                        }
                    }

                    // Wait for 1000 ms before next update
                    await Task.Delay(1000, token);
                }

                // Stop WebSocket server when done
                webSocketManager.Stop();
            
}



        private async void btnWritePLC_Click(object sender, EventArgs e)
            {   
                string plcIp = txtPlcIp.Text.Trim();
                if (mappings == null || mappings.Count == 0)
                {
                    txtStatus.Text += "\nNo mappings available. Please add mappings.";
                    return;
                }

                try
                {
                
                    if (!_plcCommunicator.Connect(plcIp, 0, 1))
                    {
                        txtStatus.Text += "\nFailed to connect to PLC.";
                        return;
                    }

                    foreach (var mapping in mappings)
                    {
                        try
                        {
                            // Fetch the value from the database for the current mapping
                            var valueFromDb = await _dbCommunicator.GetValueFromDatabaseAsync(mapping.TableName, mapping.ColumnName, mapping.RowId);

                            if (valueFromDb == null)
                            {
                                txtStatus.Text += $"\nValue not found in database for {mapping.TableName}.{mapping.ColumnName} (row {mapping.RowId})";
                                continue;
                            }

                            // Convert the value to the appropriate PLC data type
                            object valueToWrite = _plcCommunicator.ConvertValueToPLCData(valueFromDb.ToString(), mapping.DataType);

                            // Write the value to the PLC
                            _plcCommunicator.WritePLCData( mapping, valueToWrite);

                            // Log success
                            txtStatus.Text += $"\nSuccessfully wrote value to PLC for {mapping.MappingName}: {valueToWrite} {mapping.ByteOffset} {mapping.BitOffset}";
                        }
                        catch (Exception ex)
                        {
                            txtStatus.Text += $"\nError writing data for {mapping.MappingName}: {ex.Message}";
                        }
                    }

                    _plcCommunicator.Disconnect();
                    txtStatus.Text += "\nCompleted writing to PLC.";
                }
                catch (Exception ex)
                {
                    txtStatus.Text += $"\nError during PLC writing process: {ex.Message}";
                }
            }




   

        private void btnAddMapping_Click(object sender, EventArgs e)
        {
            mappings.Add(new DataMapping
            {
                MappingName = txtPlcdb.Text.Trim(),
                TableName = txtDBTableName.Text.Trim(),
                RowId = int.Parse(txtDBTableRowno.Text.Trim()),
                ColumnName = txtDBTablecolumnno.Text.Trim(),
                DbNumber = int.Parse(txtPlcdb.Text.Trim()),
                ByteOffset = int.Parse(txtbyteoffset.Text.Trim()),
                BitOffset = int.Parse(txtbitoffset.Text.Trim()),
                DataType = DataType.SelectedItem.ToString()
            });

            txtStatus.Text += $"\nMapping added: {txtPlcdb.Text.Trim()} -> {txtDBTableName.Text.Trim()}.{txtDBTablecolumnno.Text.Trim()}";
        }

        private void btnSaveMappings_Click(object sender, EventArgs e)
{
    if (mappings == null || mappings.Count == 0)
    {
        txtStatus.Text += "\nNo mappings to save. Please add some mappings.";
        return;
    }

    // Show SaveFileDialog to select file location
    SaveFileDialog saveFileDialog = new SaveFileDialog
    {
        Filter = "CSV Files (*.csv)|*.csv",
        Title = "Save Mappings as CSV",
        FileName = "mappings.csv"
    };

    if (saveFileDialog.ShowDialog() == DialogResult.OK)
    {
        string filePath = saveFileDialog.FileName;

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV header
                writer.WriteLine("MappingName,TableName,ColumnName,RowId,DataType,DBNumber,ByteOffset,BitOffset");

                // Write each mapping to the file
                foreach (var mapping in mappings)
                {
                    writer.WriteLine($"{mapping.MappingName},{mapping.TableName},{mapping.ColumnName},{mapping.RowId}," +
                                     $"{mapping.DataType},{mapping.DbNumber},{mapping.ByteOffset},{mapping.BitOffset}");
                }
            }

            txtStatus.Text += $"\nMappings saved successfully to {filePath}";
        }
        catch (Exception ex)
        {
            txtStatus.Text += $"\nError saving mappings: {ex.Message}";
        }
    }
}

       private void btnLoadMappings_Click(object sender, EventArgs e)
{
    using (OpenFileDialog openFileDialog = new OpenFileDialog())
    {
        openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var lines = File.ReadAllLines(openFileDialog.FileName).Skip(1);

                
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length != 7)
                        {
                            Console.WriteLine("Invalid row: " + line);
                                continue;
                        }

                     if (int.TryParse(parts[2], out int rowId) &&
                    int.TryParse(parts[3], out int dbNumber) &&
                    int.TryParse(parts[4], out int byteOffset) &&
                    int.TryParse(parts[5], out int bitOffset))
                {
                    mappings.Add(new DataMapping
                    {
                        TableName = parts[0],
                        ColumnName = parts[1],
                        RowId = rowId,
                        DbNumber = dbNumber,
                        ByteOffset = byteOffset,
                        BitOffset = bitOffset,
                        DataType = parts[6]
                    });
                }
                   
                }

                txtStatus.Text += $"\nSuccessfully loaded {mappings.Count} mappings from CSV.";
                
            }
            catch (Exception ex)
            {
                txtStatus.Text += $"\nError loading mappings: {ex.Message}";
            }
        }
    }
}

    }

 
} 