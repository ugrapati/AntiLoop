namespace AntiLoop
{
  partial class AntiLoop
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if(disposing&&(components!=null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AntiLoop));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
      this.grid = new System.Windows.Forms.DataGridView();
      this.pSZDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.pIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cPUDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.sl0FixDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.sl0ValDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.pmsFixDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.pmsValDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.mwmValDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.gtcValDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.qpcValDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.procListBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.on32 = new System.Windows.Forms.Button();
      this.on64 = new System.Windows.Forms.Button();
      this.hideIdle = new System.Windows.Forms.Button();
      this.idlePc = new System.Windows.Forms.TextBox();
      this.percent = new System.Windows.Forms.Label();
      this.showAll = new System.Windows.Forms.Button();
      this.x32 = new System.Windows.Forms.Button();
      this.x64 = new System.Windows.Forms.Button();
      this.delayName = new System.Windows.Forms.Label();
      this.delay = new System.Windows.Forms.TextBox();
      this.msLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.procListBindingSource)).BeginInit();
      this.SuspendLayout();
      // 
      // grid
      // 
      this.grid.AllowUserToAddRows = false;
      this.grid.AllowUserToDeleteRows = false;
      this.grid.AllowUserToResizeRows = false;
      resources.ApplyResources(this.grid, "grid");
      this.grid.AutoGenerateColumns = false;
      this.grid.BackgroundColor = System.Drawing.Color.Teal;
      this.grid.CausesValidation = false;
      this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(240)))), ((int)(((byte)(224)))));
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 10F);
      dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.CadetBlue;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pSZDataGridViewTextBoxColumn,
            this.pIDDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.cPUDataGridViewTextBoxColumn,
            this.sl0FixDataGridViewCheckBoxColumn,
            this.sl0ValDataGridViewTextBoxColumn,
            this.pmsFixDataGridViewCheckBoxColumn,
            this.pmsValDataGridViewTextBoxColumn,
            this.mwmValDataGridViewTextBoxColumn,
            this.gtcValDataGridViewTextBoxColumn,
            this.qpcValDataGridViewTextBoxColumn});
      this.grid.DataSource = this.procListBindingSource;
      this.grid.EnableHeadersVisualStyles = false;
      this.grid.GridColor = System.Drawing.Color.Black;
      this.grid.Name = "grid";
      this.grid.ReadOnly = true;
      this.grid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
      dataGridViewCellStyle13.Font = new System.Drawing.Font("Tahoma", 10F);
      this.grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle13;
      this.grid.RowHeadersVisible = false;
      this.grid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.grid.RowTemplate.Height = 24;
      this.grid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellEnter);
      this.grid.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.grid_CellMouseClick);
      this.grid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.grid_CellMouseDoubleClick);
      this.grid.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.grid_ColumnWidthChanged);
      this.grid.SelectionChanged += new System.EventHandler(this.grid_SelectionChanged);
      this.grid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grid_KeyDown);
      // 
      // pSZDataGridViewTextBoxColumn
      // 
      this.pSZDataGridViewTextBoxColumn.DataPropertyName = "SZ";
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Tahoma", 10F);
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.pSZDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
      resources.ApplyResources(this.pSZDataGridViewTextBoxColumn, "pSZDataGridViewTextBoxColumn");
      this.pSZDataGridViewTextBoxColumn.MaxInputLength = 4;
      this.pSZDataGridViewTextBoxColumn.Name = "pSZDataGridViewTextBoxColumn";
      this.pSZDataGridViewTextBoxColumn.ReadOnly = true;
      this.pSZDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      // 
      // pIDDataGridViewTextBoxColumn
      // 
      this.pIDDataGridViewTextBoxColumn.DataPropertyName = "PID";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.pIDDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
      resources.ApplyResources(this.pIDDataGridViewTextBoxColumn, "pIDDataGridViewTextBoxColumn");
      this.pIDDataGridViewTextBoxColumn.MaxInputLength = 9;
      this.pIDDataGridViewTextBoxColumn.Name = "pIDDataGridViewTextBoxColumn";
      this.pIDDataGridViewTextBoxColumn.ReadOnly = true;
      this.pIDDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      // 
      // nameDataGridViewTextBoxColumn
      // 
      this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
      this.nameDataGridViewTextBoxColumn.DataPropertyName = "NAME";
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle4.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.nameDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
      resources.ApplyResources(this.nameDataGridViewTextBoxColumn, "nameDataGridViewTextBoxColumn");
      this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
      this.nameDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // cPUDataGridViewTextBoxColumn
      // 
      this.cPUDataGridViewTextBoxColumn.DataPropertyName = "CPU";
      dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle5.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.cPUDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
      resources.ApplyResources(this.cPUDataGridViewTextBoxColumn, "cPUDataGridViewTextBoxColumn");
      this.cPUDataGridViewTextBoxColumn.MaxInputLength = 6;
      this.cPUDataGridViewTextBoxColumn.Name = "cPUDataGridViewTextBoxColumn";
      this.cPUDataGridViewTextBoxColumn.ReadOnly = true;
      this.cPUDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      // 
      // sl0FixDataGridViewCheckBoxColumn
      // 
      this.sl0FixDataGridViewCheckBoxColumn.DataPropertyName = "sl0Fix";
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle6.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
      dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.sl0FixDataGridViewCheckBoxColumn.DefaultCellStyle = dataGridViewCellStyle6;
      resources.ApplyResources(this.sl0FixDataGridViewCheckBoxColumn, "sl0FixDataGridViewCheckBoxColumn");
      this.sl0FixDataGridViewCheckBoxColumn.MaxInputLength = 3;
      this.sl0FixDataGridViewCheckBoxColumn.Name = "sl0FixDataGridViewCheckBoxColumn";
      this.sl0FixDataGridViewCheckBoxColumn.ReadOnly = true;
      this.sl0FixDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      // 
      // sl0ValDataGridViewTextBoxColumn
      // 
      this.sl0ValDataGridViewTextBoxColumn.DataPropertyName = "sl0Val";
      dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle7.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.sl0ValDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle7;
      resources.ApplyResources(this.sl0ValDataGridViewTextBoxColumn, "sl0ValDataGridViewTextBoxColumn");
      this.sl0ValDataGridViewTextBoxColumn.Name = "sl0ValDataGridViewTextBoxColumn";
      this.sl0ValDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // pmsFixDataGridViewCheckBoxColumn
      // 
      this.pmsFixDataGridViewCheckBoxColumn.DataPropertyName = "pmsFix";
      dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle8.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
      dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.pmsFixDataGridViewCheckBoxColumn.DefaultCellStyle = dataGridViewCellStyle8;
      resources.ApplyResources(this.pmsFixDataGridViewCheckBoxColumn, "pmsFixDataGridViewCheckBoxColumn");
      this.pmsFixDataGridViewCheckBoxColumn.MaxInputLength = 3;
      this.pmsFixDataGridViewCheckBoxColumn.Name = "pmsFixDataGridViewCheckBoxColumn";
      this.pmsFixDataGridViewCheckBoxColumn.ReadOnly = true;
      this.pmsFixDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      // 
      // pmsValDataGridViewTextBoxColumn
      // 
      this.pmsValDataGridViewTextBoxColumn.DataPropertyName = "pmsVal";
      dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle9.Font = new System.Drawing.Font("Consolas", 9.75F);
      dataGridViewCellStyle9.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.pmsValDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle9;
      resources.ApplyResources(this.pmsValDataGridViewTextBoxColumn, "pmsValDataGridViewTextBoxColumn");
      this.pmsValDataGridViewTextBoxColumn.Name = "pmsValDataGridViewTextBoxColumn";
      this.pmsValDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // mwmValDataGridViewTextBoxColumn
      // 
      this.mwmValDataGridViewTextBoxColumn.DataPropertyName = "mwmVal";
      dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle10.Font = new System.Drawing.Font("Consolas", 9.75F);
      dataGridViewCellStyle10.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle10.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle10.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.mwmValDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle10;
      resources.ApplyResources(this.mwmValDataGridViewTextBoxColumn, "mwmValDataGridViewTextBoxColumn");
      this.mwmValDataGridViewTextBoxColumn.Name = "mwmValDataGridViewTextBoxColumn";
      this.mwmValDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // gtcValDataGridViewTextBoxColumn
      // 
      this.gtcValDataGridViewTextBoxColumn.DataPropertyName = "gtcVal";
      dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle11.Font = new System.Drawing.Font("Consolas", 9.75F);
      dataGridViewCellStyle11.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle11.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle11.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.gtcValDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle11;
      resources.ApplyResources(this.gtcValDataGridViewTextBoxColumn, "gtcValDataGridViewTextBoxColumn");
      this.gtcValDataGridViewTextBoxColumn.Name = "gtcValDataGridViewTextBoxColumn";
      this.gtcValDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // qpcValDataGridViewTextBoxColumn
      // 
      this.qpcValDataGridViewTextBoxColumn.DataPropertyName = "qpcVal";
      dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(16)))));
      dataGridViewCellStyle12.Font = new System.Drawing.Font("Consolas", 9.75F);
      dataGridViewCellStyle12.ForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.RoyalBlue;
      dataGridViewCellStyle12.SelectionForeColor = System.Drawing.Color.White;
      dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.qpcValDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle12;
      resources.ApplyResources(this.qpcValDataGridViewTextBoxColumn, "qpcValDataGridViewTextBoxColumn");
      this.qpcValDataGridViewTextBoxColumn.Name = "qpcValDataGridViewTextBoxColumn";
      this.qpcValDataGridViewTextBoxColumn.ReadOnly = true;
      // 
      // procListBindingSource
      // 
      this.procListBindingSource.DataSource = typeof(AntiLoop.procList);
      // 
      // on32
      // 
      this.on32.BackColor = System.Drawing.Color.Transparent;
      this.on32.BackgroundImage = global::AntiLoop.Properties.Resources.small0bars;
      resources.ApplyResources(this.on32, "on32");
      this.on32.CausesValidation = false;
      this.on32.FlatAppearance.BorderSize = 0;
      this.on32.ForeColor = System.Drawing.Color.MidnightBlue;
      this.on32.Name = "on32";
      this.on32.UseVisualStyleBackColor = false;
      this.on32.Click += new System.EventHandler(this.on32_Click);
      // 
      // on64
      // 
      this.on64.BackColor = System.Drawing.Color.Transparent;
      this.on64.BackgroundImage = global::AntiLoop.Properties.Resources.small0bars;
      resources.ApplyResources(this.on64, "on64");
      this.on64.CausesValidation = false;
      this.on64.FlatAppearance.BorderSize = 0;
      this.on64.ForeColor = System.Drawing.Color.MidnightBlue;
      this.on64.Name = "on64";
      this.on64.UseVisualStyleBackColor = false;
      this.on64.Click += new System.EventHandler(this.on64_Click);
      // 
      // hideIdle
      // 
      this.hideIdle.BackColor = System.Drawing.Color.Transparent;
      resources.ApplyResources(this.hideIdle, "hideIdle");
      this.hideIdle.FlatAppearance.BorderSize = 0;
      this.hideIdle.ForeColor = System.Drawing.Color.Black;
      this.hideIdle.Name = "hideIdle";
      this.hideIdle.UseVisualStyleBackColor = false;
      this.hideIdle.Click += new System.EventHandler(this.hideIdle_Click);
      // 
      // idlePc
      // 
      this.idlePc.BackColor = System.Drawing.Color.White;
      this.idlePc.BorderStyle = System.Windows.Forms.BorderStyle.None;
      resources.ApplyResources(this.idlePc, "idlePc");
      this.idlePc.ForeColor = System.Drawing.Color.Black;
      this.idlePc.Name = "idlePc";
      this.idlePc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.idlePc_KeyDown);
      this.idlePc.Leave += new System.EventHandler(this.idlePc_Leave);
      this.idlePc.Validating += new System.ComponentModel.CancelEventHandler(this.idlePc_Validating);
      // 
      // percent
      // 
      resources.ApplyResources(this.percent, "percent");
      this.percent.CausesValidation = false;
      this.percent.ForeColor = System.Drawing.Color.Black;
      this.percent.Name = "percent";
      // 
      // showAll
      // 
      resources.ApplyResources(this.showAll, "showAll");
      this.showAll.BackColor = System.Drawing.Color.Transparent;
      this.showAll.FlatAppearance.BorderSize = 0;
      this.showAll.ForeColor = System.Drawing.Color.Black;
      this.showAll.Name = "showAll";
      this.showAll.UseVisualStyleBackColor = false;
      this.showAll.Click += new System.EventHandler(this.showAll_Click);
      // 
      // x32
      // 
      resources.ApplyResources(this.x32, "x32");
      this.x32.BackColor = System.Drawing.Color.Transparent;
      this.x32.FlatAppearance.BorderSize = 0;
      this.x32.ForeColor = System.Drawing.Color.Black;
      this.x32.Name = "x32";
      this.x32.UseVisualStyleBackColor = false;
      this.x32.Click += new System.EventHandler(this.x32_Click);
      // 
      // x64
      // 
      resources.ApplyResources(this.x64, "x64");
      this.x64.BackColor = System.Drawing.Color.Transparent;
      this.x64.FlatAppearance.BorderSize = 0;
      this.x64.ForeColor = System.Drawing.Color.Black;
      this.x64.Name = "x64";
      this.x64.UseVisualStyleBackColor = false;
      this.x64.Click += new System.EventHandler(this.x64_Click);
      // 
      // delayName
      // 
      resources.ApplyResources(this.delayName, "delayName");
      this.delayName.BackColor = System.Drawing.Color.Transparent;
      this.delayName.CausesValidation = false;
      this.delayName.Cursor = System.Windows.Forms.Cursors.Default;
      this.delayName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.delayName.ForeColor = System.Drawing.Color.Black;
      this.delayName.Name = "delayName";
      // 
      // delay
      // 
      resources.ApplyResources(this.delay, "delay");
      this.delay.BackColor = System.Drawing.Color.White;
      this.delay.CausesValidation = false;
      this.delay.ForeColor = System.Drawing.Color.Black;
      this.delay.Name = "delay";
      this.delay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.delay_KeyDown);
      this.delay.Leave += new System.EventHandler(this.delay_Leave);
      // 
      // msLabel
      // 
      resources.ApplyResources(this.msLabel, "msLabel");
      this.msLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.msLabel.ForeColor = System.Drawing.Color.Black;
      this.msLabel.Name = "msLabel";
      // 
      // AntiLoop
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.BackgroundImage = global::AntiLoop.Properties.Resources.Controller64;
      this.CausesValidation = false;
      this.Controls.Add(this.on32);
      this.Controls.Add(this.on64);
      this.Controls.Add(this.hideIdle);
      this.Controls.Add(this.idlePc);
      this.Controls.Add(this.percent);
      this.Controls.Add(this.showAll);
      this.Controls.Add(this.x32);
      this.Controls.Add(this.x64);
      this.Controls.Add(this.delayName);
      this.Controls.Add(this.delay);
      this.Controls.Add(this.msLabel);
      this.Controls.Add(this.grid);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.White;
      this.HelpButton = true;
      this.Name = "AntiLoop";
      this.Activated += new System.EventHandler(this.AntiLoop_Activated);
      this.Deactivate += new System.EventHandler(this.AntiLoop_Deactivate);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AntiLoop_KeyDown);
      ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.procListBindingSource)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button on32;
    private System.Windows.Forms.Button on64;
    private System.Windows.Forms.Button hideIdle;
    private System.Windows.Forms.TextBox idlePc;
    private System.Windows.Forms.Label percent;
    private System.Windows.Forms.Button x32;
    private System.Windows.Forms.Button x64;
    private System.Windows.Forms.Button showAll;
    private System.Windows.Forms.Label delayName;
    private System.Windows.Forms.TextBox delay;
    private System.Windows.Forms.Label msLabel;
    private System.Windows.Forms.DataGridView grid;
    private System.Windows.Forms.BindingSource procListBindingSource;
    private System.Windows.Forms.DataGridViewTextBoxColumn pSZDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn pIDDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn cPUDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn sl0FixDataGridViewCheckBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn sl0ValDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn pmsFixDataGridViewCheckBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn pmsValDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn mwmValDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn gtcValDataGridViewTextBoxColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn qpcValDataGridViewTextBoxColumn;
  }
}
