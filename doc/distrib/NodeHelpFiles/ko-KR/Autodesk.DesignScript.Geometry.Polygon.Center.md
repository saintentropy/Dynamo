## 상세
Polygon Center는 모서리의 평균값을 사용하여 지정된 다각형의 중심을 구합니다. 오목한 다각형의 경우 중심이 실제로는 다각형 외부에 있을 수 있습니다. 아래 예에서는 먼저 Point By Cylindrical Coordinates에 대한 입력으로 사용할 임의 각도와 반지름의 리스트를 생성합니다. 먼저 각도를 분류하여 결과 다각형이 각도가 증가하는 순서대로 연결되도록 합니다. 이렇게 하면 다각형이 자체 교차하지 않습니다. 그런 다음 중심을 사용하여 점의 평균을 구하고 다각형 중심을 구할 수 있습니다.
___
## 예제 파일

![Center](./Autodesk.DesignScript.Geometry.Polygon.Center_img.jpg)

