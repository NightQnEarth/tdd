﻿using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace TagsCloudVisualization.Tests
{
    public class Tests
    {
        private const double Precision = 0.7072; // sqrt(2)/2.
        private static readonly Point origin = Point.Empty;

        [Test]
        public void CircularCloudLayouterConstructor_GetCenterPoint() =>
            Assert.DoesNotThrow(() => new CircularCloudLayouter(origin));

        [Test]
        public void PutNextRectangle_OnZeroSize_ThrowArgumentException() =>
            Assert.Throws<ArgumentException>(
                () => new CircularCloudLayouter(origin).PutNextRectangle(new Size(0, 0)));

        [Test]
        public void GetRectangleCenter_RandomRectangle(
            [Random(1)] int xLocation, [Random(1)] int yLocation,
            [Random(1, 1000, 1)] int width, [Random(1, 1000, 1)] int height)
        {
            var location = new Point(xLocation, yLocation);
            var rectangle = new Rectangle(location, new Size(width, height));

            rectangle.GetRectangleCenter().GetDistanceToPoint(location).Should()
                     .BeApproximately(rectangle.GetCircumscribedCircleRadius(), Precision);
        }

        [TestCase(0, 0, TestName = "WithOriginAsCenter")]
        [TestCase(2, -3, TestName = "WithOddCenterCoordinates")]
        public void GetRectangleWithCenterInThePoint(int xCenter, int yCenter)
        {
            var center = new Point(xCenter, yCenter);
            var centeredRectangle = center.GetRectangleWithCenterInThePoint(new Size(2, 3));

            centeredRectangle.Location.GetDistanceToPoint(center).Should()
                             .BeApproximately(centeredRectangle.GetCircumscribedCircleRadius(), Precision);
        }

        [TestCase(12, 8, TestName = "OnEvenWidthAndHeight_ReturnsWithZeroPrecision")]
        [TestCase(10, 5, TestName = "OnEvenWidthAndOddHeight_ReturnsPrecision")]
        [TestCase(1, 1, TestName = "OnOddWidthAndHeight_ReturnsWithPrecision")]
        public void PutNextRectangle_OnFirstSize_ReturnsRectangleInCenter(int width, int height)
        {
            var firstRectangle = new CircularCloudLayouter(origin).PutNextRectangle(new Size(width, height));

            origin.GetDistanceToPoint(firstRectangle.Location)
                  .Should().BeApproximately(firstRectangle.GetCircumscribedCircleRadius(), Precision);
        }

        [Test]
        public void PutNextRectangle_OnSecondSize_ReturnsNotIntersectedRectangle()
        {
            var circularCloudLayouter = new CircularCloudLayouter(origin);
            var firstRectangle = circularCloudLayouter.PutNextRectangle(new Size(10, 5));
            var secondRectangle = circularCloudLayouter.PutNextRectangle(new Size(7, 3));

            firstRectangle.IntersectsWith(secondRectangle).Should().BeFalse();
        }

        [Test]
        public void PutNextRectangle_OnALotOfCalls_ReturnsNotIntersectedRectangles()
        {
            var circularCloudLayouter = new CircularCloudLayouter(origin);
            var random = new Random(1);
            var rectangles = Enumerable.Range(0, 100)
                                       .Select(i => circularCloudLayouter.PutNextRectangle(
                                                   new Size(random.Next(1, 50), random.Next(1, 50))))
                                       .ToArray();

            CheckIfAnyIntersects(rectangles).Should().BeFalse();
        }

        private static bool CheckIfAnyIntersects(Rectangle[] rectangles)
        {
            for (int i = 0; i < rectangles.Length; i++)
                for (int j = i + 1; j < rectangles.Length; j++)
                    if (rectangles[i].IntersectsWith(rectangles[j]))
                        return true;
            return false;
        }
    }
}