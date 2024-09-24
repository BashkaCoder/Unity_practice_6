# Optimization 🫡 in Unity
## Task:
Оптимизировать существующий проект. В качестве проекта выбран пет-проект RPG игры. 

Версия игры до оптимизации находится в ветке [Old](https://www.google.ru/?hl=ru).
Оптимизированная версия игры находится в ветке [New](https://www.google.ru/?hl=ru)


## Выполнение:
Сравнение скорости отрисовки кадра в `Profiler Analyzer`.
В среднем кадр стал отрисовываться на 185% быстрее. То есть, средний FPS увеличился почти в 3 раза. с 18.25ms до 6.37ms
![ProfilerAnalyzer_comparison](https://github.com/user-attachments/assets/bae4b7de-0301-432d-81aa-c8271081be80)

Сравнение памяти в `Memory Profiler`
![MemoryProfiler_comparison](https://github.com/user-attachments/assets/9f8a50e9-8f12-4789-970c-a673589e07ab)

## Использованные инструменты
- [ProjectAuditor](https://github.com/Unity-Technologies/ProjectAuditor)
- Unity Profiler
- Profiler Analyzer
- Unity Memory Profiler
- Unity Frame Debugger

## Список проведенной работы:
- Отключил RaycastTarget на элементах UI, которые не предусматривают взаимодействие с мышкой
- Поместил спрайты UI в spriteAtlas
- Немного переписал скрипт воды: Оптимизация создаваемых камер и текстур, кеширование ссылок
- PLayerController.cs: кеширование ссылки на камеру
- Враги обновляют свои анматоры только если видны(_renderer.IsVisible)
- Убрал Debug.Log() и тд
- PlayerController.cs: [SphereCastAll](https://docs.unity3d.com/ScriptReference/Physics.SphereCastAll.html) => [SphereCastNonAlloc](https://docs.unity3d.com/ScriptReference/Physics.SphereCastNonAlloc.html)
- ProjectSettings/Quality/ShadowMaskMode: Distance Shadowmask => Shadowmask
- ProjectSettings/Quality/Shadow Distance: изменил значение, чтобы отрисовывались только тени в пределах frustrum камеры
- Всем источникам света(компонент Light) заменил Mode c "Realtime" на "Mixed"
- Добавил static Batching
- Изменил Terrain. Увеличил "Pixer Error": 5 => 20
- Изменил Terrain. Уменьшил "Base map Distance": 1000 => 300
- Сделал все окружение статическим для Static Batching
- Включил GPU Instancing у деревьев и прочих пропов на сцене
- Для Ambient треков изменил Load Type: "Decompress on Load" => "Streaming"
- Включил "Prebake Collision Meshes" в Player Settings
- Project Settings/Physics: Свел матрицу коллизий до необходимого минимума
- Изменил Fixed Timestep до 0.06(Так как таргет - 60фпс)
- По предложениям [ProjectAuditor](https://github.com/Unity-Technologies/ProjectAuditor) изменил немного код и настройки импорта ассетов(модели, аудио, текстуры)
- Изменил Scripting Backend: c Mono на IL2CPP

## В чем я преисполнился
- #### Оптимизация размера:
  Использование SpriteAtlas для объединения UI-спрайтов сократило вес ассетов и улучшило производительность.
- #### Оптимизация памяти:
  Изменение режима загрузки аудио на "Streaming" и уменьшение Base Map Distance для Terrain снижает использование оперативной памяти.
- #### Оптимизация CPU/GPU:
  Переход на кеширование ссылок, переход на SphereCastNonAlloc, уменьшение количества активных анимаций и снижение числа обрабатываемых теней повысили производительность.
- #### Оптимизация загрузки: 
  Включение "Prebake Collision Meshes" ускоряет время загрузки коллизий.
- #### Батчинг оптимизация:
  Включение Static Batching для окружения и GPU Instancing для деревьев и объектов на сцене уменьшает количество draw calls.
- #### Оптимизации UI:
  Перевод спрайтов UI в атласы и отключение ненужных RaycastTargets снизили затраты на отрисовку и обработку.
- #### Draw calls, SetPass:
  Уменьшение draw calls с помощью батчинга и инстансинга привело к снижению SetPass вызовов, что уменьшило нагрузку на GPU.
