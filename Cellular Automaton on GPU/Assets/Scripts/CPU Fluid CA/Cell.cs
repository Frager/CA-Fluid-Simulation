using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{

    //basic CA element
    public struct Cell
    {
        //volume describes the combined amount of all contents in the cell
        public int volume;
        //list of contents in cell. Index equals id and number represents amount
        public int[] content;
        //cells temperature in celcius
        public float temperature;

        public Cell(int elementAmount, int maxVolume)
        {
            this.volume = 0;
            content = new int[elementAmount];
            temperature = 20f;
        }

        //creates an exact copy of the cell
        //used to copy the new generation to current ceneration before computing updates
        public Cell copyCell()
        {
            Cell copy = new Cell();
            copy.volume = volume;
            copy.temperature = temperature;
            int[] copyContent = new int[content.Length];
            for (int i = 0; i < content.Length; ++i)
            {
                copyContent[i] = content[i];
            }
            copy.content = copyContent;
            return copy;
        }

        public int getLightestContent()
        {
            for (int id = 0; id < content.Length; ++id)
            {
                if (content[id] > 0) return id;
            }
            return -1;
        }

        public int getHeaviestContent()
        {
            for (int id = content.Length - 1; id >= 0; --id)
            {
                if (content[id] > 0) return id;
            }
            return -1;
        }

        public void addContent(int amount)
        {
            volume += amount;
        }

        public void setContent(int amount)
        {
            volume = amount;
        }

        public void addContent(int amount, int elementIndex)
        {
            content[elementIndex] += amount;
            volume += amount;
        }

        public int getVolume()
        {
            return volume;
        }

        public int takeContent(int amount)
        {
            volume -= amount;
            return volume;
        }
        
        public void takeContent(int amount, int elementIndex)
        {
            content[elementIndex] -= amount;
            volume -= amount;
        }

        public void deleteContent()
        {
            volume = 0;
        }

        static public implicit operator Color(Cell cell)
        {
            Color color = new Color(1, 1, 1, 0.1f);
            if (cell.volume < 0)
            {
                color = new Color(1f, 1f, 1f, 1f);
            }
            else if (cell.volume >= 1)
            {
                color = new Color((cell.content[1] == 0)? 0: 1, (cell.content[0] == 0) ? 0 : 1, (cell.content[2] == 0) ? 0 : 1, 1f);
            }
            return color;
        }

        public Color getTempColor()
        {
            Color color;
            if (volume != 0)
            {
                if (temperature > 100)
                    color = new Color(1f-(temperature) / 100f, ((temperature) / 100f), 0, 1f);
                color = new Color(temperature / 100f, 0, 1f - (temperature / 100f), 1f);
            }
            else
            {
                if(temperature > 100)
                    color = new Color(1f-(temperature) / 100f, ((temperature) / 100f), 0, 0.1f);
                color = new Color(temperature / 100f, 0, 1f - (temperature / 100f), 0.1f);
            }
            return color;
        }

    }

    public struct GPUCell
    {
        public float[] content;

        public GPUCell Copy()
        {
            GPUCell copy = new GPUCell();
            content.CopyTo(copy.content, 0);
            return copy;
        }

        static public implicit operator Color(GPUCell cell)
        {
            return new Color(1f, 1f, 1f, 0.1f);
        }
    }
}
