
using System;
using System.IO;
public class CdgFileIoStream : IDisposable
{

	#region "Declarations"

		#endregion
	private Stream m_file;

	#region "Public Methods"
	public CdgFileIoStream()
	{
		m_file = null;
	}

	public int read(ref byte[] buf, int buf_size)
	{
		return m_file.Read(buf, 0, buf_size);
	}

	public int write(ref byte[] buf, int buf_size)
	{
		m_file.Write(buf, 0, buf_size);
		return 1;
	}

	public int seek(int offset, SeekOrigin whence)
	{
		return (int)m_file.Seek((long)offset, whence);
	}

	public int eof()
	{
		if ((m_file.Position >= m_file.Length)) {
			return 1;
		} else {
			return 0;
		}
	}

	public int getsize()
	{
		return (int)m_file.Length;
	}

	public bool open(string filename)
	{
		close();
		m_file = new FileStream(filename, FileMode.Open, FileAccess.Read);
		return (m_file != null);
	}

	public void close()
	{
		if ((m_file != null)) {
			m_file.Close();
			m_file = null;
		}
	}

    #endregion

    // To detect redundant calls
    private bool disposedValue = false;

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                m_file.Dispose();
            }
            m_file = null;
            m_file = null;
        }
        this.disposedValue = true;
    }

    #region " IDisposable Support "
    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik, @toddanglin
//Facebook: facebook.com/telerik
//=======================================================
