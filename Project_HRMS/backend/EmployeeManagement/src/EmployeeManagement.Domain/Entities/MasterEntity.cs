namespace EmployeeManagement.Domain.Entities;

public abstract class MasterEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
}

/// <summary>Implemented by master data that belongs to a parent master (State→Country, City→State).</summary>
public interface IChildMaster
{
    int ParentId { get; set; }
}
