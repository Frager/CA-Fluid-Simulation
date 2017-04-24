using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public enum Direction
    {
        xPos, xNeg, yPos, yNeg, zPos, zNeg, none
    }

    public struct Cell
    {
        public int maxVolume;
        public int volume;
        Direction direction;
        public int moveElementId;
        public int[] content;
        public float temperature;

        public Cell(int elementAmount, int maxVolume)
        {
            this.maxVolume = maxVolume;
            this.volume = 0;
            this.direction = Direction.none;
            moveElementId = -1;
            content = new int[elementAmount];
            temperature = 20f;
        }

        public Cell copyCell()
        {
            Cell copy = new Cell();
            copy.maxVolume = maxVolume;
            copy.volume = volume;
            copy.setDirection(direction);
            copy.moveElementId = moveElementId;
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

        public void setDirection(Direction dir)
        {
            direction = dir;
        }

        public Direction getDirection()
        {
            return direction;
        }

        static public implicit operator Color(Cell cell)
        {
            //if (cell.volume == 0) return new Color(1, 1, 1, .1f);
            //else return new Color(0, 0, 1, 1f);

            Color color = new Color(1, 1, 1, 0.1f);
            if (cell.volume < 0)
            {
                color = new Color(1f, 1f, 1f, 1f);
            }
            else if (cell.volume > cell.maxVolume)
            {
                color = new Color(0f, 0f, 0f, 1f);
            }
            else if (cell.volume == cell.maxVolume)
            {
                color = new Color(0.8f * (float)(cell.content[1] + 4) / (float)cell.maxVolume, 0.8f * (float)(cell.content[2] + 4) / (float)cell.maxVolume, 0.8f * (float)(cell.content[0] + 4) / (float)cell.maxVolume, 1f);
            }
            else if (cell.volume >= 1)
            {
                color = new Color((float)(cell.content[1] + 4) / (float)(cell.maxVolume), (float)(cell.content[2] + 4) / (float)(cell.maxVolume), (float)(cell.content[0] + 4) / (float)cell.maxVolume, 1f);
            }
            if (cell.direction != Direction.none) color.r += 0.3f;
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
