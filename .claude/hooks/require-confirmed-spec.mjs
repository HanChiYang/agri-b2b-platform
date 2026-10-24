// Claude Code PreToolUse hook：規格要經過使用者確認，才能開始寫測試和程式。原因見 docs/adr/0013。
// - 在 feature/ branch 上改 src/、tests/ 時，對應的規格（docs/specs/<branch 名稱去掉 feature/>.md）狀態要是「已確認」或「已完成」
// - 把規格的狀態改成「已確認」或「已完成」時，跳出確認視窗，由使用者決定是否允許
// 開發流程見 CLAUDE.md。
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import path from 'node:path';

const FEATURE_BRANCH = /^feature\/(.+)/;
const GUARDED_DIRS = new Set(['src', 'tests']);
const NOT_SPECS = new Set(['_template.md', 'readme.md']);
const CONFIRMED_STATUSES = new Set(['已確認', '已完成']);
// 規格檔開頭的「- 狀態：草稿」
const STATUS_LINE = /^- 狀態：\s*(\S+)/m;
// 修改的內容可能只有一部分，所以不要求行首的「- 」；寬鬆一點，最多是多跳一次確認視窗
const STATUS_IN_EDIT = /狀態：\s*(\S+)/;

const input = await readInput();
const projectDir = process.env.CLAUDE_PROJECT_DIR || input.cwd;
const target = input.tool_input?.file_path ?? input.tool_input?.notebook_path;
if (!projectDir || !target) process.exit(0);

const parts = path.relative(path.resolve(projectDir), path.resolve(projectDir, target))
  .split(path.sep)
  .map((part) => part.toLowerCase());

if (parts.length === 3 && parts[0] === 'docs' && parts[1] === 'specs' && parts[2].endsWith('.md') && !NOT_SPECS.has(parts[2])) {
  checkStatusChange(path.resolve(projectDir, target));
} else if (GUARDED_DIRS.has(parts[0])) {
  checkSpecConfirmed();
}

process.exit(0);

function checkStatusChange(specFile) {
  const next = STATUS_IN_EDIT.exec(input.tool_input?.new_string ?? input.tool_input?.content ?? '')?.[1];
  if (!CONFIRMED_STATUSES.has(next) || CONFIRMED_STATUSES.has(readStatus(specFile))) return;

  ask(`Claude 要把規格 ${path.basename(specFile)} 的狀態改成「${next}」。` +
    '請確認你已經同意這份規格再允許；允許之後，Claude 就會開始寫測試和程式。');
}

function checkSpecConfirmed() {
  // fix/ branch 不一定有規格，不檢查；其他 branch 由 require-feature-branch.mjs 擋下
  const name = FEATURE_BRANCH.exec(currentBranch(projectDir) ?? '')?.[1];
  if (!name) return;

  const spec = `docs/specs/${name}.md`;
  const status = readStatus(path.join(projectDir, spec));
  if (status === undefined) {
    deny(`還沒有這個 branch 的規格 ${spec}，不能修改 src/、tests/。請先照 CLAUDE.md 的流程寫規格，請使用者確認。`);
  }
  if (!CONFIRMED_STATUSES.has(status)) {
    deny(`規格 ${spec} 的狀態是「${status ?? '沒有寫'}」，使用者確認之前不能修改 src/、tests/。` +
      '請停下來請使用者確認規格；使用者同意之前，不要自己修改規格的狀態。');
  }
}

// 讀規格的狀態。檔案不存在回傳 undefined，沒有寫狀態回傳 null
function readStatus(file) {
  let text;
  try {
    text = readFileSync(file, 'utf8');
  } catch {
    return undefined;
  }
  return STATUS_LINE.exec(text)?.[1] ?? null;
}

async function readInput() {
  const chunks = [];
  for await (const chunk of process.stdin) chunks.push(chunk);
  const text = Buffer.concat(chunks).toString('utf8').trim();
  return text ? JSON.parse(text) : {};
}

function currentBranch(dir) {
  try {
    return execFileSync('git', ['-C', dir, 'branch', '--show-current'], {
      encoding: 'utf8',
      stdio: ['ignore', 'pipe', 'ignore'],
    }).trim();
  } catch {
    return null;
  }
}

function deny(reason) {
  decide('deny', reason);
}

function ask(reason) {
  decide('ask', reason);
}

function decide(permissionDecision, reason) {
  process.stdout.write(JSON.stringify({
    hookSpecificOutput: {
      hookEventName: 'PreToolUse',
      permissionDecision,
      permissionDecisionReason: reason,
    },
  }));
  process.exit(0);
}
