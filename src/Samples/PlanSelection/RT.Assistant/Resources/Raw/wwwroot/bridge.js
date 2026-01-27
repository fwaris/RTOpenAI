function toBase64(str) {
  const bytes = new TextEncoder().encode(str);
  const binString = Array.from(bytes, byte => String.fromCodePoint(byte)).join('');
  return btoa(binString);
}


//async input request
window.addEventListener(
    "HybridWebViewMessageReceived",
    async function (e) {
        let v = JSON.parse(e.detail.message);
        //showAnswer(e.detail.message); //shows received data on web page (for debugging)
        let cCode = v.predicates;
        let qCode = v.query;
        let ans = await evaluateQuery(cCode, qCode);
        let ansstr = JSON.stringify(ans);
        let ansstrBase64 = toBase64(ansstr);
        await window.HybridWebView.InvokeDotNet('GotMessage', [ansstrBase64]);
    });

async function getClauses() {
    if (globalThis.clauses) {
        return globalThis.clauses;
    }
    const response = await fetch('/plan_clauses.pl');
    globalThis.clauses = await response.text();
    return globalThis.clauses;
}

getClauses(); //prime cache

function accumulateAnswers(session, finalCallback) {
    let results = [];
    function loop() {
        session.answer({
            success: function(answer) {
                results.push(session.format_answer(answer));
                loop(); // get the next answer
            },
            fail: function() {
                // No more answers: return the accumulated list.
                finalCallback(null, results);
            },
            error: function(err) {
                finalCallback(err);
            },
            limit: function() {
                finalCallback(new Error("query exceeded resource limits"));
            }
        });
    }
    loop();
}


async function evaluateQuery(consult,query) {
    let session = pl.create()
    session.setMaxInferences(1000)
    session.thread.setMaxInferences(1000)
    let pcode = await getClauses() + '\n' + consult;
    let ans =
        session.promiseConsult(pcode)
            .then(() => session.promiseQuery(query))
            .then(() =>
                new Promise((resolve, reject) =>
                {
                    accumulateAnswers(session, ((err,answers) => {
                        if (err) {
                            reject(err);
                        } else {
                            resolve(answers)
                        }
                    }))
                }))
            .then(answers => {
                let rslt = answers.map(a => a.toString())
                return {
                    error: false,
                    result: rslt
                }
            })
            .catch(err => {
                return {
                    error: true,
                    result: [err ? err.toString() : "undefined"]
                }
            });
    return ans;
}

async function evaluateQuery2(consult,query) {
    let session = pl.create()
    session.setMaxInferences(1000)
    let pcode = await getClauses() + '\n' + consult;
    let ans =
        session.promiseConsult(pcode)
            .then(() => session.promiseQuery(query))
            .then(() => {
                // Use an immediately invoked async function to collect all answers.
                return (async () => {
                    const answers = [];
                    for await (let answer of session.promiseAnswers()) {
                        answers.push(session.format_answer(answer));
                    }
                    return answers;
                })();
            })
            .then(answers => {
                let rslt = answers.filter(Boolean).join('\n');
                return {
                    error: false,
                    result: rslt
                }
            })
            .catch(err => {
                return {
                    error: true,
                    result: err ? err.toString() : "undefined"
                }
            });
    return ans;
}

function test() {
    evaluateQuery("","plan(T,_,_,_).")
        .then(results => showAnswer(JSON.stringify(results)))
}

function testError() {
    evaluateQuery("","plan(_,T,_,_,_).")
        .then(results => showAnswer(JSON.stringify(results)))
}

function showAnswer(rslt) {
    let el = document.getElementById('results');
    el.insertAdjacentHTML('beforeend', '<li>' + rslt + '</li>')
}

function clearResults() {
    let el = document.getElementById('results');
    el.innerHTML = ''
}

async function runQuery() {
    let elC = document.getElementById('consultText');
    let elQ = document.getElementById('queryText');
    let ans = await evaluateQuery(elC.value,elQ.value)
    showAnswer(ans.result);
}    