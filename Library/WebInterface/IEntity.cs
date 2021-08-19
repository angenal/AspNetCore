namespace WebInterface
{
    /// <summary>Interface for an entity with an ID. </summary>
    public interface IEntity<TIdentity>
    {
        /// <summary>Gets the ID of the entity. </summary>
        TIdentity Id { get; }
    }
}
