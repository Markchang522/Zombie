# Contributing (貢獻指南)
> 在 vsc 打開，按Ctrl+k ，放開後再按下 v 檢視

此文件包含git 會用到的指令、commit 格式以及changelog (開發日誌)格式。
請大家遵循下列格式

## git 指令
> 請先在電腦下載 git
### 初始化

1. 在 unity 創建 Zombie 專案

2. 在 cmd cd到專案資料夾

3. 建立 .git
```text
git init
```

4. 與 GitHub 連結
```text
git remote add origin https://github.com/Markchang522/Zombie.git
```

5. 下載目前進度
```text
git pull origin main --allow-unrelated-histories
```

### 開發中commit

> - **先將專案 pull 再開始做**
> - **做到一個進度就 commit 一次**

1. 下載目前進度
```text
git pull origin main
```
如果電腦有做東西但還沒 pull，改成
```text
git pull origin main --allow-unrelated-histories
```


2. 將所有更新檔加入站存區
```text
git add .
```
出現 waring 不要理它，重新 add 一次

3. 存檔並寫下 Commit 訊息。
```text
git commit -m "feat(auth): 新增登入頁面"
```

5. 將所有 Commit 上傳 GitHub
```text
git push origin main
```
如果出現
```text
>error: failed to push some refs to 'https://github.com/Markchang522/Zombie.git'
```
先輸入
```text
git branch -M main
```
## Commit 格式
本專案採用 **Conventional Commits (慣例式提交)** 規範。請確保你的 Commit 訊息格式如下：

```text
<type (類型)>(<scope (影響的範圍)>): <subject (簡短說明具體做了什麼)>
```

例如
```text
git commit -m "feat(auth): 新增登入頁面"
```

### 常用的 Type
- feat (新增)：新功能的加入。

- changed (變更)：現有功能的行為調整。

- deprecated (即將棄用)：未來版本會移除的功能。

- removed (移除)：已刪除的功能。

- fixed (修正)：Bug 修復。

- security (安全性)：安全性漏洞修補。

- chore (雜項)：不知道歸哪類就寫這個。

### 常用的 Scope
- (player)：跟玩家有關。

- (ui)：使用者介面。

- (scene)：場景。

- (audio)：音效與音樂。

- (anim)：動畫。

- (ai)：敵人、NPC 相關。

- (other)：不知道歸哪類就寫這個。

## Changelog (開發日誌)

## - Y-M-D(by...)
### Added
- 新增的功能

### Changed
- 更改的內容

### Fixed
- 修復的 Bug

### Note
- 備註

## 範例：- 2026-11-19(By 張晨煥)
### Added
- 建置場景(未完成)

### Changed
- 更改玩家移動腳本(完成)

### Fixed
- 修復玩家無法移動 Bug (完成)

### Note
- 新的開發日誌放在上面