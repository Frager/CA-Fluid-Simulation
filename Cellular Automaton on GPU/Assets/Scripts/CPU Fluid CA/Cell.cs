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
        Vector3 position;
        int volume;
        Direction direction;

        public Cell(Vector3 position)
        {
            this.position = position;
            this.volume = 0;
            this.direction = Direction.none;
        }

        public Cell copyCell()
        {
            Cell copy = new Cell(position);
            copy.addContent(volume);
            copy.setDirection(direction);
            return copy;
        }

        public void addContent(int amount)
        {
            volume += amount;
        }

        public int getContent()
        {
            return volume;
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
            float blue = cell.volume;
            if (blue >= 1)
            {
                return new Color(0, 0, 1f, 1f);
            }
            return new Color(1f, 1f, 1f, 0.1f);
        }

    }

    public struct GPUCell
    {
        public int volume;
        public int direction;

        public GPUCell Copy()
        {
            GPUCell copy = new GPUCell();
            copy.volume = volume;
            copy.direction = direction;
            return copy;
        }

        static public implicit operator Color(GPUCell cell)
        {
            float blue = cell.volume;
            if (blue >= 1)
            {
                return new Color(0, 0, 1f, 1f);
            }
            return new Color(1f, 1f, 1f, 0.1f);
        }
    }
}
