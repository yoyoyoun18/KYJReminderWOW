### claude code rules
 - claude code는 상급자이고 사용자는 부하직원이라고 생각하고 대한다. 따라서 어떠한 구조, 로직, 코드 제안에 있어서 무조건 긍정적인 반응 보다는 최대한 예외와 더 좋은 케이스가 없는지 찾고 그것에 대해 조언해주려 노력해야함
 - 제안해주는 답변은 최대한 간단하고 명료하게, 그리고 코드는 어렵고 복잡한 코드보다는 최대한 단순하고 간단한 로직과 문법을 사용하여 제안해주어야한다. 만약 어려운 문법을 사용할경우 해당 문법에 대해 소개를 해줘야함.

## Code Architecture Guidelines

### Sequence-Based Function Design

#### 기본 원칙
- **시퀀스 함수는 단계별 명확성을 우선시한다**
- **과도한 추상화보다는 가독성과 직관성을 선택한다**
- **리팩토링은 실제 문제가 있을 때만 수행한다**

#### 시퀀스 함수 작성 가이드라인

##### 1. Enum 기반 단계 정의
```csharp
/// <summary>
/// 시퀀스의 각 단계를 명확히 정의
/// </summary>
public enum SequenceStep
{
    Initialize,
    ValidateInput,
    ProcessData,
    GenerateOutput,
    Finalize,
    Complete,
    Error
}
```

##### 2. 컨텍스트 구조체 선언
```csharp
/// <summary>
/// 시퀀스 실행 중 사용되는 모든 변수와 상태를 담는 구조체
/// </summary>
public class SequenceContext
{
    // === Input Parameters (읽기 전용 입력값) ===
    public readonly InputDataType InputData;
    public readonly ConfigurationType Config;
    
    // === Working Variables (단계간 공유 작업 변수) ===
    public List<ProcessedItem> ProcessedItems { get; set; }
    public Dictionary<string, object> WorkingData { get; set; }
    public string CurrentProfileName { get; set; }
    public List<int> TargetIndices { get; set; }
    
    // === State Management (상태 관리) ===
    public SequenceStep CurrentStep { get; set; }
    public SequenceStep PreviousStep { get; set; }
    public Exception LastError { get; set; }
    public bool HasError => LastError != null;
    
    // === Output Results (최종 결과) ===
    public ResultType Result { get; set; }
    public List<OutputItem> OutputItems { get; set; }
    
    /// <summary>
    /// 컨텍스트 초기화
    /// </summary>
    public SequenceContext(InputDataType input, ConfigurationType config)
    {
        InputData = input;
        Config = config;
        ProcessedItems = new List<ProcessedItem>();
        WorkingData = new Dictionary<string, object>();
        TargetIndices = new List<int>();
        OutputItems = new List<OutputItem>();
        CurrentStep = SequenceStep.Initialize;
    }
    
    /// <summary>
    /// 상태 전환 메서드
    /// </summary>
    public void TransitionTo(SequenceStep nextStep)
    {
        PreviousStep = CurrentStep;
        CurrentStep = nextStep;
    }
}
```

##### 3. Case문 기반 시퀀스 구현 (권장)
```csharp
public ReturnType seqFunctionName(InputType input, ConfigType config)
{
    // 컨텍스트 초기화
    var context = new SequenceContext(input, config);
    
    // 단계별 실행
    while (context.CurrentStep != SequenceStep.Complete && 
           context.CurrentStep != SequenceStep.Error)
    {
        switch (context.CurrentStep)
        {
            case SequenceStep.Initialize:
                // 초기화 로직
                InitializeSequence(context);
                context.TransitionTo(SequenceStep.ValidateInput);
                break;
                
            case SequenceStep.ValidateInput:
                // 입력값 검증 로직
                if (ValidateInput(context))
                    context.TransitionTo(SequenceStep.ProcessData);
                else
                    context.TransitionTo(SequenceStep.Error);
                break;
                
            case SequenceStep.ProcessData:
                // 핵심 처리 로직
                ProcessData(context);
                context.TransitionTo(SequenceStep.GenerateOutput);
                break;
                
            case SequenceStep.GenerateOutput:
                // 결과 생성 로직
                GenerateOutput(context);
                context.TransitionTo(SequenceStep.Finalize);
                break;
                
            case SequenceStep.Finalize:
                // 마무리 로직
                FinalizeSequence(context);
                context.TransitionTo(SequenceStep.Complete);
                break;
                
            default:
                context.TransitionTo(SequenceStep.Error);
                break;
        }
    }
    
    // 결과 반환
    return context.HasError ? DefaultResult() : context.Result;
}

// 각 단계별 구현 함수들
private void InitializeSequence(SequenceContext context) { /* 구현 */ }
private bool ValidateInput(SequenceContext context) { /* 구현 */ return true; }
private void ProcessData(SequenceContext context) { /* 구현 */ }
private void GenerateOutput(SequenceContext context) { /* 구현 */ }
private void FinalizeSequence(SequenceContext context) { /* 구현 */ }
```

##### 4. 간단한 goto 방식 (단순한 경우)
```csharp
public ReturnType seqSimpleFunction(parameters)
{
    const int STEP_INITIALIZE = 0;
    const int STEP_PROCESS = 1;
    const int STEP_FINALIZE = 2;
    
    int nStep = STEP_INITIALIZE;
    
    switch(nStep)
    {
        case STEP_INITIALIZE:
            // 명확한 초기화 로직
            if (조건충족) goto case STEP_PROCESS;
            else return DefaultResult();
            
        case STEP_PROCESS:
            // 핵심 처리 로직
            goto case STEP_FINALIZE;
            
        case STEP_FINALIZE:
            // 마무리 로직
            break;
    }
    
    return result;
}
```

#### 함수 명명 규칙

##### 시퀀스 함수
```csharp
// seq + 주요기능 + 세부사항
seqMakeMergeReviewImage()           // ✅ 명확함
seqProcessInspectionData()          // ✅ 명확함
seqValidateAndSaveResults()         // ✅ 명확함
seqHandle()                         // ❌ 모호함
```

##### 단계별 함수
```csharp
// 동사 + 명사 형태
InitializeSequence()                // ✅ 초기화 단계
ValidateInputData()                 // ✅ 검증 단계
ProcessImageData()                  // ✅ 처리 단계
GenerateReviewImage()               // ✅ 생성 단계
```

#### 에러 처리 패턴
```csharp
private void ProcessStepWithErrorHandling(SequenceContext context)
{
    try
    {
        // 실제 처리 로직
        DoActualWork(context);
    }
    catch (Exception ex)
    {
        context.LastError = ex;
        LogError($"Step {context.CurrentStep} failed", ex);
        context.TransitionTo(SequenceStep.Error);
    }
}
```

#### 로깅 패턴
```csharp
private void LogStepTransition(SequenceContext context, string message = null)
{
    string log = $"[Sequence] {context.PreviousStep} → {context.CurrentStep}";
    if (!string.IsNullOrEmpty(message))
        log += $" - {message}";
    
    Console.WriteLine(log);
    EnqueueEventLog(log);
}
```

#### 사용 가이드라인
- **10줄 이하 단계**: goto 방식 사용
- **10-30줄 단계**: enum + switch 방식 사용
- **30줄 이상 단계**: 각 단계를 별도 함수로 분리
- **복잡한 상태 관리**: 컨텍스트 구조체 활용
- **에러 처리 중요**: try-catch + 상태 전환 패턴 적용