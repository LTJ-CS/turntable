// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: activity.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace ClientProto {

  /// <summary>Holder for reflection information generated from activity.proto</summary>
  public static partial class ActivityReflection {

    #region Descriptor
    /// <summary>File descriptor for activity.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ActivityReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5hY3Rpdml0eS5wcm90bxILQ2xpZW50UHJvdG8aDGNvbW1vbi5wcm90byJC",
            "CiRBY3Rpdml0eUVuZXJneVN1cHBseVJld2FyZEdldFJlcXVlc3QiGgoBThII",
            "CgR6ZXJvEAASCwoGT3BDb2RlEKFRIpACCiVBY3Rpdml0eUVuZXJneVN1cHBs",
            "eVJld2FyZEdldFJlc3BvbnNlEk8KCWVycm9yQ29kZRgBIAEoDjI8LkNsaWVu",
            "dFByb3RvLkFjdGl2aXR5RW5lcmd5U3VwcGx5UmV3YXJkR2V0UmVzcG9uc2Uu",
            "RXJyb3JDb2RlEiQKBWl0ZW1zGAIgAygLMhUuQ2xpZW50UHJvdG8uSXRlbUlu",
            "Zm8SKQoKZmluYWxJdGVtcxgDIAMoCzIVLkNsaWVudFByb3RvLkl0ZW1JbmZv",
            "IhoKAU4SCAoEemVybxAAEgsKBk9wQ29kZRCiUSIpCglFcnJvckNvZGUSCwoH",
            "U3VjY2VzcxAAEg8KC0NvbmZpZ0Vycm9yEAFCFVoTLi9wcm90b2J1Zjtwcm90",
            "b2J1ZmIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::ClientProto.CommonReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::ClientProto.ActivityEnergySupplyRewardGetRequest), global::ClientProto.ActivityEnergySupplyRewardGetRequest.Parser, null, null, new[]{ typeof(global::ClientProto.ActivityEnergySupplyRewardGetRequest.Types.N) }, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ClientProto.ActivityEnergySupplyRewardGetResponse), global::ClientProto.ActivityEnergySupplyRewardGetResponse.Parser, new[]{ "ErrorCode", "Items", "FinalItems" }, null, new[]{ typeof(global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.N), typeof(global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode) }, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// 体力补给奖励领取 mark 暂时无限领取
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ActivityEnergySupplyRewardGetRequest : pb::IMessage<ActivityEnergySupplyRewardGetRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ActivityEnergySupplyRewardGetRequest> _parser = new pb::MessageParser<ActivityEnergySupplyRewardGetRequest>(() => new ActivityEnergySupplyRewardGetRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ActivityEnergySupplyRewardGetRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ClientProto.ActivityReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetRequest(ActivityEnergySupplyRewardGetRequest other) : this() {
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetRequest Clone() {
      return new ActivityEnergySupplyRewardGetRequest(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ActivityEnergySupplyRewardGetRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ActivityEnergySupplyRewardGetRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ActivityEnergySupplyRewardGetRequest other) {
      if (other == null) {
        return;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the ActivityEnergySupplyRewardGetRequest message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      public enum N {
        [pbr::OriginalName("zero")] Zero = 0,
        [pbr::OriginalName("OpCode")] OpCode = 10401,
      }

    }
    #endregion

  }

  /// <summary>
  /// 体力补给奖励领取 
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ActivityEnergySupplyRewardGetResponse : pb::IMessage<ActivityEnergySupplyRewardGetResponse>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ActivityEnergySupplyRewardGetResponse> _parser = new pb::MessageParser<ActivityEnergySupplyRewardGetResponse>(() => new ActivityEnergySupplyRewardGetResponse());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ActivityEnergySupplyRewardGetResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ClientProto.ActivityReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetResponse(ActivityEnergySupplyRewardGetResponse other) : this() {
      errorCode_ = other.errorCode_;
      items_ = other.items_.Clone();
      finalItems_ = other.finalItems_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ActivityEnergySupplyRewardGetResponse Clone() {
      return new ActivityEnergySupplyRewardGetResponse(this);
    }

    /// <summary>Field number for the "errorCode" field.</summary>
    public const int ErrorCodeFieldNumber = 1;
    private global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode errorCode_ = global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode ErrorCode {
      get { return errorCode_; }
      set {
        errorCode_ = value;
      }
    }

    /// <summary>Field number for the "items" field.</summary>
    public const int ItemsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::ClientProto.ItemInfo> _repeated_items_codec
        = pb::FieldCodec.ForMessage(18, global::ClientProto.ItemInfo.Parser);
    private readonly pbc::RepeatedField<global::ClientProto.ItemInfo> items_ = new pbc::RepeatedField<global::ClientProto.ItemInfo>();
    /// <summary>
    /// 获得的奖励
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::ClientProto.ItemInfo> Items {
      get { return items_; }
    }

    /// <summary>Field number for the "finalItems" field.</summary>
    public const int FinalItemsFieldNumber = 3;
    private static readonly pb::FieldCodec<global::ClientProto.ItemInfo> _repeated_finalItems_codec
        = pb::FieldCodec.ForMessage(26, global::ClientProto.ItemInfo.Parser);
    private readonly pbc::RepeatedField<global::ClientProto.ItemInfo> finalItems_ = new pbc::RepeatedField<global::ClientProto.ItemInfo>();
    /// <summary>
    /// 领奖后的道具，用于客户端同步数据
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::ClientProto.ItemInfo> FinalItems {
      get { return finalItems_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ActivityEnergySupplyRewardGetResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ActivityEnergySupplyRewardGetResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ErrorCode != other.ErrorCode) return false;
      if(!items_.Equals(other.items_)) return false;
      if(!finalItems_.Equals(other.finalItems_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ErrorCode != global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success) hash ^= ErrorCode.GetHashCode();
      hash ^= items_.GetHashCode();
      hash ^= finalItems_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ErrorCode != global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success) {
        output.WriteRawTag(8);
        output.WriteEnum((int) ErrorCode);
      }
      items_.WriteTo(output, _repeated_items_codec);
      finalItems_.WriteTo(output, _repeated_finalItems_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ErrorCode != global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success) {
        output.WriteRawTag(8);
        output.WriteEnum((int) ErrorCode);
      }
      items_.WriteTo(ref output, _repeated_items_codec);
      finalItems_.WriteTo(ref output, _repeated_finalItems_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ErrorCode != global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) ErrorCode);
      }
      size += items_.CalculateSize(_repeated_items_codec);
      size += finalItems_.CalculateSize(_repeated_finalItems_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ActivityEnergySupplyRewardGetResponse other) {
      if (other == null) {
        return;
      }
      if (other.ErrorCode != global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode.Success) {
        ErrorCode = other.ErrorCode;
      }
      items_.Add(other.items_);
      finalItems_.Add(other.finalItems_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ErrorCode = (global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode) input.ReadEnum();
            break;
          }
          case 18: {
            items_.AddEntriesFrom(input, _repeated_items_codec);
            break;
          }
          case 26: {
            finalItems_.AddEntriesFrom(input, _repeated_finalItems_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ErrorCode = (global::ClientProto.ActivityEnergySupplyRewardGetResponse.Types.ErrorCode) input.ReadEnum();
            break;
          }
          case 18: {
            items_.AddEntriesFrom(ref input, _repeated_items_codec);
            break;
          }
          case 26: {
            finalItems_.AddEntriesFrom(ref input, _repeated_finalItems_codec);
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the ActivityEnergySupplyRewardGetResponse message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      public enum N {
        [pbr::OriginalName("zero")] Zero = 0,
        [pbr::OriginalName("OpCode")] OpCode = 10402,
      }

      public enum ErrorCode {
        [pbr::OriginalName("Success")] Success = 0,
        [pbr::OriginalName("ConfigError")] ConfigError = 1,
      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
