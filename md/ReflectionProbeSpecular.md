##  ReflectionProbeSpecular
 PBR에서 라이트맵을 구우면 스펙큘러가 사라집니다.  
 아쉬운데로 리플렉션 프로브를 이용해서 스펙큘러를 살려보는 방법이 있습니다.  
 라이트 위치에 구를 하나 만들어서 에미션으로 발광시켜서 리플렉션 프로브를 베이크를 하면 그럭저럭 괜찮게 나오게 됩니다.  
그런데 베이크 하기 위해서 사용된 포인트 라이트 숫자가 많으면 하나하나 구를 배치하기에는 귀찮아서 자동으로 생성해주는 툴을 챗지피티를 이용해서 만들었습니다. 저는 코딩 할줄 모르거든요..
 
## 사용방법
![easyme](/md/img/ReflectionProbeSpecular_01.png)   
 - Create Sphere : 씬에 활성화 된 포인트 라이트 위치에 구를 생성합니다.
 - Remove Sphere : 생성된 구를 모두 제거해줍니다.
 - Bake Reflection Probes : 리플렉션 프로브를 베이크 해줍니다.
 - Scale : 구의 크기 조절
 - Intensity : 에미션 세기 조절


  
![easyme](/md/img/ReflectionProbeSpecular_02.png)

Create Sphere 를 누르면 구가 생성됩니다. 포인트 라이트의 색상을 자동으로 추출해서 가져옵니다.
Scale 과 Intensity를 조절하면 바로바로 변경할수 있습니다.  
Bake Reflection Probes를 눌러서 베이크 해줍니다.

![easyme](/md/img/ReflectionProbeSpecular_03.png)

잘 구워졌는지 확인했으면 Remove Sphere를 눌러주면 생성된 구를 모두 지워줍니다.

![easyme](/md/img/ReflectionProbeSpecular_04.png)