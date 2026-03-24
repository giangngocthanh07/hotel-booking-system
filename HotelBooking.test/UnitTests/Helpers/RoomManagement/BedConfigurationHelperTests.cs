using HotelBooking.application.DTOs.Hotel;
using FluentAssertions;
using Docker.DotNet.Models;

public class BedConfigurationHelperTests
{
    // Guard Tests
    [Fact]
    public void FormatBedConfiguration_EmptyList_ShouldReturnEmptyString()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>();


        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().BeEmpty();

    }

    // Single Bed Tests
    [Fact]
    public void FormatBedConfiguration_OneSingleBed_ShouldReturnSingle()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Single", Quantity = 1 }
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Single");
    }


    [Fact]
    public void FormatBedConfiguration_TwoSingleBeds_ShouldReturnTwin()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Single", Quantity = 2 } // Assuming BedTypeId 1 corresponds to "Single Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Twin");
    }

    [Fact]
    public void FormatBedConfiguration_ThreeSingleBeds_ShouldReturnTriple()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Single", Quantity = 3 }
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Triple");
    }

    [Fact]
    public void FormatBedConfiguration_FourSingleBeds_ShouldReturnQuadruple()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Single", Quantity = 4 }
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Quadruple");
    }

    [Fact]
    public void FormatBedConfiguration_GreaterThanFourSingleBeds_ShouldReturnCountWithPlural()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Bunk", Quantity = 5 }
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("5 Bunks");
    }

    // Other Bed Types Tests
    [Fact]
    public void FormatBedConfiguration_OneBedQuantity_ShouldReturnBedName()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Sofa", Quantity = 1 } // Assuming BedTypeId 5 corresponds to "Sofa Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Sofa");

    }

    [Fact]
    public void FormatBedConfiguration_TwoBedQuantity_ShouldReturnTwinBedName()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Murphy", Quantity = 2 } // Assuming BedTypeId 6 corresponds to "Murphy Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Twin Murphy");
    }

    [Fact]
    public void FormatBedConfiguration_ThreeBedQuantity_ShouldReturnTripleBedName()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Futon", Quantity = 3 } // Assuming BedTypeId 7 corresponds to "Futon"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Triple Futon");
    }

    [Fact]
    public void FormatBedConfiguration_FourBedQuantity_ShouldReturnQuadrupleBedName()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Canopy", Quantity = 4 } // Assuming BedTypeId 8 corresponds to "Canopy Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Quadruple Canopy");

    }

    [Fact]
    public void FormatBedConfiguration_GreaterThanFourBedQuantity_ShouldReturnCountWithPluralBedName()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Loft", Quantity = 5 } // Assuming BedTypeId 9 corresponds to "Loft Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("5 Lofts");
    }

    // Combination
    [Fact]
    public void FormatBedConfiguration_OneBedNameAndTwoSingle_ShouldReturnBedNameAndTwin()
    {
        // Arrange
        var bedTypes = new List<BedTypeNameDTO>
        {
            new BedTypeNameDTO { Name = "Daybed", Quantity = 1 }, // Assuming BedTypeId 10 corresponds to "Daybed"
            new BedTypeNameDTO { Name = "Single", Quantity = 2 } // Assuming BedTypeId 1 corresponds to "Single Bed"
        };

        // Act
        var result = BedConfigurationHelper.FormatBedConfiguration(bedTypes);

        // Assert
        result.Should().Be("Daybed and Twin");
    }

}