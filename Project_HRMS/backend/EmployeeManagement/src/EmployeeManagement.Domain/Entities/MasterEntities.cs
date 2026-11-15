namespace EmployeeManagement.Domain.Entities;

public sealed class Department : MasterEntity
{
}

public sealed class Designation : MasterEntity
{
}

public sealed class EmployeeType : MasterEntity
{
}

public sealed class Relationship : MasterEntity
{
}

public sealed class Country : MasterEntity
{
    public List<State> States { get; set; } = [];
}

public sealed class State : MasterEntity, IChildMaster
{
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
    public List<City> Cities { get; set; } = [];

    // Explicit implementation keeps this out of the EF model.
    int IChildMaster.ParentId
    {
        get => CountryId;
        set => CountryId = value;
    }
}

public sealed class City : MasterEntity, IChildMaster
{
    public int StateId { get; set; }
    public State State { get; set; } = null!;

    int IChildMaster.ParentId
    {
        get => StateId;
        set => StateId = value;
    }
}
