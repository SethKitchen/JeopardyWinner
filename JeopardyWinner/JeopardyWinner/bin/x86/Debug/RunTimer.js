var s = document.createElement("script"); s.type = "text/javascript"; s.src = "https://www.jeopardy.com/Assets/jeopardy/easyxdm/exdm_helper.js"; $("head").append(s);

s.src = "https://www.jeopardy.com/Assets/jeopardy/js/main.js"; $("head").append(s);

s.src = "https://www.jeopardy.com/Assets/jeopardy/js/practice_test_launcher.js"; $("head").append(s);

backendUserCheck(); test_launcher.init(); test_launcher.testWindow = window;

$('body').append('<div data - testfile = "/Assets/jeopardy/json/test_practice_2011.json" data - testid = "1" class="button launch_practice_test" data-testtype="adult">Take the practice test</div>');

var button = $('.launch_practice_test'); test_launcher.testQuestionsFile = button.attr('data-testfile'); test_launcher.testType = button.attr('data-testtype'); test_launcher.testTitle = button.attr('data-testtitle'); test_launcher.testID = button.attr('data-testid');

testModule.parentRef = test_launcher;

testModule.deleteLocalStorage();
testModule.checkLocalStorage();
testModule.getLocalStorage();
testModule.testID = testModule.parentRef.getTestID();
testModule.testType = testModule.parentRef.getTestType();
testModule.testFile = testModule.parentRef.getFile();
testModule.hardwareTest = testModule.parentRef.checkIfDryRun();

testModule.sendTestStart();

trace = function () { };

trace = {
    output: true,
    queue: [],

    push: function (data) {
        trace.queue.push(data);
        if (trace.output) {
            trace.post();
        }
        return
    },

    post: function () {
        $.each(trace.queue, function (i, arr) {
            trace.log(arr);
        });
        trace.queue = [];
        return
    },

    dump: function () {
        trace.post();
        return
    },

    log: function (data) {
        if (!window.console) { console = { log: function () { } } };
        console.log(data);
        return
    }
};

testModule.loadQuiz();
testModule.parentRef = test_launcher;
testModule.startDate = testModule.parentRef.getStartDateInMS();