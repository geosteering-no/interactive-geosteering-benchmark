using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ResistivitySimulator
{
    interface IModelDrawer<T>
    {
        /// <summary>
        /// Pan and zoom does not matter because graphics contains everything
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="model"></param>
        /// <param name="transperancy"></param>
        void DrawOnGraphics(Graphics gr, T model, int transperancy);
    }
}
