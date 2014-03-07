using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    public interface IFileStorage
    {
        void Write(IFile file, string content);
        string ReadText(IFile file);
        /// <summary>
        /// Copy the content of the file into the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        /// <returns>[True] if the content has been copied else [False]</returns>
        bool CopyTo(IFile file, Stream destination);

        /// <summary>
        /// Open a stream on the file
        /// </summary>
        /// <param name="addView">if set to <c>true</c> [add view].</param>
        /// <returns></returns>
        Stream OpenRead(IFile file);

        /// <summary>
        /// Opens a write only stream on the repository file.
        /// CALL Flush() ON THE STREAM BEFORE EXITING THE USING BLOCK!
        /// </summary>
        /// <returns>The stream</returns>
        Stream OpenWrite(IFile file);


        void Write(IFile file, Stream stream);
    }
}
