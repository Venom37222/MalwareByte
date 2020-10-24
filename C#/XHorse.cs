private Assembly CompileCSharp(string strFilePath)
{
    if (strFilePath == null)
        return null;
    StreamReader sr = new StreamReader(strFilePath);
    string strSource = sr.ReadToEnd();
    sr.Close();
    CodeDomProvider cc = new CSharpCodeProvider();
    CompilerParameters cp = new CompilerParameters();
    foreach (AssemblyName assemblyName in 
    Assembly.GetEntryAssembly().GetReferencedAssemblies())
        cp.ReferencedAssemblies.Add(assemblyName.Name + ".dll");
    cp.GenerateInMemory = true;
    CompilerResults cr = cc.CompileAssemblyFromSource(cp, strSource);

    StringBuilder sb = new StringBuilder();

    if (cr.Errors.HasErrors || cr.Errors.HasWarnings)
    {
        foreach (CompilerError err in cr.Errors)
            sb.AppendLine(err.ToString());
        MessageBox.Show(sb.ToString(), "Error", 
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
        return null;
    }
    return cr.CompiledAssembly;
}
private void Execute(object FilePath)
{
    Assembly assembly = CompileCSharp(FilePath as string);
    if (assembly == null)
        return;
        
    foreach (Type t in assembly.GetTypes())
    {
        MethodInfo info = t.GetMethod("Main",
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static);

        if (info == null)
            continue;

        object[] parameters = new object[]
        { new string[] { FilePath as string } };
        info.Invoke(null, parameters);
    }
}
static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        string strFilePath;
        if (args.Length == 0)
        {
            string strDirectory =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            strFilePath = Path.Combine(strDirectory, "SelfReplication.cs");
            if (!File.Exists(strFilePath))
            {
                StreamWriter sw = new StreamWriter(strFilePath);
                sw.Write(Program.MYSELF);
                sw.Close();

                StreamReader sr = new StreamReader(strFilePath);
                string strCode = sr.ReadToEnd();
                sr.Close();

                int intI = strCode.IndexOf(" " + "MYSELF");
                if (intI > 0)
                    strCode = strCode.Substring(0, intI + 7) + 
                              ";\r\n\t}\r\n}\r\n";

                string strInsertCode = "MYSELF=@\"" + 
                       strCode.Replace("\"", "\"\"") + "\";";

                strCode = strCode.Replace("MYSELF" + ";", strInsertCode);

                sw = new StreamWriter(strFilePath);
                sw.Write(strCode);
                sw.Close();

                return;
            }
        }
        else
        {
            strFilePath = args[0];
        }
        Application.Run(new SelfReplication(strFilePath));
    }
    public static string MYSELF=@".......";
}
private string strFilePath;
public SelfReplication(string strFilePath)
{
    this.strFilePath = strFilePath;
    this.button1 = new Button();
    this.button1.Location = new Point(75, 25);
    this.button1.Size = new Size(100, 25);
    this.button1.Text = "Replicate";
    this.button1.Click += new System.EventHandler(this.button1_Click);
    this.ClientSize = new Size(250, 75);
    this.Controls.Add(this.button1);
    this.Text = "SelfReplication";
}

private void button1_Click(object sender, EventArgs e)
{
    Thread thread = new Thread(new ParameterizedThreadStart(Execute));
    thread.Name = "Execute";
    thread.IsBackground = true;
    thread.Start(strFilePath);
}
