namespace BracketPipe
{
  /// <summary>
  /// How to handle a specific tag
  /// </summary>
  public enum SanitizeBehavior
  {
    /// <summary>
    /// Remove the tag entirely
    /// </summary>
    Discard,
    /// <summary>
    /// Allow the tag without any alterations
    /// </summary>
    Allow,
    /// <summary>
    /// Encode the tag as text
    /// </summary>
    Encode
  }
}
