# Sword-Wielding
unity实现任意方向的挥剑攻击动画
## 实现流程
基于playable混合不同帧的不同动画以达到任意角度，将一次挥剑分为蓄力和挥击两个部分，两部分独立根据鼠标位置计算混合比例，鼠标快速移动时从蓄力切换为攻击，并且通过二分法计算两部分的转换衔接点。
## 演示