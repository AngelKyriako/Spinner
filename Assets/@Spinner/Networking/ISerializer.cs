public interface ISerializer {

    M Deserialize<M>(string serializedData) where M : class;

    string Serialize(object model);

}
