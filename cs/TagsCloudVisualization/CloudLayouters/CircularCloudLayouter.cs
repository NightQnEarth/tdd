﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TagsCloudVisualization.Extensions;

namespace TagsCloudVisualization.CloudLayouters
{
    public class CircularCloudLayouter : ICloudLayouter
    {
        private const double AzimuthDelta = Math.PI / 18;

        private readonly Size centralOffset;
        private readonly List<Rectangle> rectangles = new List<Rectangle>();
        private readonly ArchimedeanSpiral spiral = new ArchimedeanSpiral(AzimuthDelta);

        public CircularCloudLayouter(Point center) => centralOffset = new Size(center);

        public Rectangle PutNextRectangle(Size rectangleSize)
        {
            if (rectangleSize.IsEmpty)
                throw new ArgumentException("Passed argument is empty.", nameof(rectangleSize));

            Rectangle newRectangle = Rectangle.Empty;

            foreach (var currentArchimedeanSpiralPoint in spiral.GetPoints())
            {
                Point newRectangleLocation = currentArchimedeanSpiralPoint;

                newRectangle = new Rectangle(newRectangleLocation, rectangleSize);

                if (!newRectangle.IntersectsWith(rectangles)) break;

                if (currentArchimedeanSpiralPoint.X <= 0)
                {
                    newRectangle.Offset(new Point(-rectangleSize.Width, -rectangleSize.Height));

                    if (!newRectangle.IntersectsWith(rectangles)) break;
                }
            }

            if (rectangles.Count == 0)
            {
                var firstRectangleOffset = new Point(-rectangleSize.Width / 2,
                                                     -rectangleSize.Height / 2);
                newRectangle.Offset(firstRectangleOffset);
            }

            newRectangle = MoveRectangleToOrigin(newRectangle, rectangles);

            rectangles.Add(newRectangle);

            return newRectangle.CreateMovedCopy(centralOffset);
        }

        private static Rectangle MoveRectangleToOrigin(Rectangle rectangle, IReadOnlyCollection<Rectangle> rectangles)
        {
            if (rectangle.Contains(0, 0)) return rectangle;

            var xDelta = -Math.Sign(rectangle.X + rectangle.Width / 2);
            var yDelta = -Math.Sign(rectangle.Y + rectangle.Height / 2);

            var sizes = new[] { new Size(xDelta, 0), new Size(0, yDelta) }.Where(size => !size.IsEmpty);

            for (int i = 0; i < 2; i++)
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var size in sizes)
                    while (true)
                    {
                        var movedRectangle = rectangle.CreateMovedCopy(size);
                        if (movedRectangle.IntersectsWith(rectangles) ||
                            XDistanceToCenter(movedRectangle) == 0 && size.Width != 0 ||
                            YDistanceToCenter(movedRectangle) == 0 && size.Height != 0)
                            break;
                        rectangle = movedRectangle;
                    }

            return rectangle;

            static int XDistanceToCenter(Rectangle rect) => rect.X + rect.Width / 2;
            static int YDistanceToCenter(Rectangle r) => r.Y + r.Height / 2;
        }
    }
}