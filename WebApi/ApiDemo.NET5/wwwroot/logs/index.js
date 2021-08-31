
function logLoad() {
    var btn = $('#logMoreBtn'), tbl = $('#logListTbl');
    var p = parseInt(btn.attr('page'));
    if (isNaN(p) || p < 1) p = 1;
    btn.attr('page', p).hide();
    $.ajax({
        type: 'GET',
        url: '/api/log/exception/query/' + p,
        contentType: 'application/json',
        success: (data) => {
            //console.log(data);
            for (var i = 0; i < data.rows.length; i++) {
                var item = data.rows[i];
                var tpl = '<tr><td style="cursor:pointer" onclick="$(\'#' + (item.id.replace('-', '')) + '\').toggle()"><span class="text-muted bg-info">' + (item.path) + '</span><br><span class="text-danger bg-warning">' + (item.trace) + '</span></td><td><span class="text-danger">' + (item.message) + '</span></td><td><span class="text-muted">' + (item.time) + '</span></td></tr>';
                tpl += '<tr id="' + (item.id.replace('-', '')) + '" style="display:none"><td colspan="3" align="left"><pre>' + (item.content) + '</pre></td></tr>';
                //console.log(tpl);
                var row = $(tpl);
                tbl.append(row);
            }
            if (data.total > p) btn.attr('page', (p + 1)).show();
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}

//const MillisecondsOfOneDay = 24 * 60 * 60 * 1000;

//$('.sortby[value='+(location.hash=='#site'?'site':'time')+']').attr('checked',true);

//for(var name in $.easing){
//  $('.debug-easing').append('<option>'+name+'</option>');
//}
//$('.debug-easing option:contains(easeInOutQuart)').attr('selected',true);

//function htmlEncode(str) {
//  var tmpDiv = htmlEncode.tmpDiv = htmlEncode.tmpDiv || document.createElement('div');
//  tmpDiv.textContent = str;
//  return tmpDiv.innerHTML;
//}

//function getCnWeekday(wday){
//  return '日一二三四五六'[wday];
//}

//function getCnDate(date, config) {
//  config = config || {
//      year: true,
//      month: true,
//      day: true,
//      week: true
//  };
//  return (config.year ? date.getFullYear() + '年' : '') +
//         (config.month ? (date.getMonth()+1) + '月' : '') +
//         (config.day ? date.getDate() + '日' : '') +
//         (config.week ? '-星期' + getCnWeekday(date.getDay()) : '');
//}

//function getHeaderDate(date) {
//  var strDate =  getCnDate(date);
//  var offday = Math.floor(new Date(date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate() + ' GMT+0').getTime() / MillisecondsOfOneDay)
//               - Math.floor(new Date().getTime() / MillisecondsOfOneDay);
//  if (offday === 0) {
//    strDate += '<span>（今天）</span>';
//  } else if (offday === -1) {
//    strDate += '<span>（昨天）</span>';
//  }
//  return strDate;
//}

//var historys = new History({
//    sortby: function(){
//      return $('.sortby:checked').val();
//    },
//    searchtext: function(){
//      return $('#search-field').val();
//    },
//    offday: function(){
//      return $('.scroll-area li.justcur a').attr('offday');
//    },
//    stage: '.content ul',
//    tmplSiteHeader: '#tmplSiteHeader',
//    tmplHistoryItem: '#tmplHistoryItem'
//});

//CETH.on(historys, 'AfterRender', function(e){
//  $(window).trigger('resize');
//  $('#clear-pages').attr('disabled', !e.items.length);
//});

//$('#search-field').on('mousedown', function(e) {
//  var jself = $(e.target);
//  if (jself.val().length) {
//    setTimeout(function() {
//      if (!jself.val().length) {
//        if (window.last_date_link) {
//          var lastLink = $(window.last_date_link).trigger('mousedown');
//          var lastLinkAsOl = lastLink.parent().parent();
//          if (!lastLinkAsOl.is(':visible')) {
//            lastLinkAsOl.prev().children().trigger('click');
//          }

//        }
//      }
//    },200);
//  }
//});

//var reWheel = 0;
//$('.content').on('mousewheel', function(e) {
//  if(e.originalEvent.wheelDeltaY>0 && this.scrollTop==0 && $('.scrollable-sections li.cur').prev().length){
//    reWheel++;
//    if(reWheel>10){
//      $('.scrollable-sections li.cur').prev().find('a').trigger('mousedown');
//      $('.content-up-suggest,.content-down-suggest').remove();
//      reWheel=0;
//    }
//    if(!$('.content-up-suggest').length){
//      $('<div/>').addClass('content-up-suggest').appendTo(this);
//    }else{
//      $('.content-up-suggest').css('opacity', reWheel/10).html(10-reWheel);
//    }
//  }else if(e.originalEvent.wheelDeltaY<0 && this.scrollTop + this.offsetHeight == this.scrollHeight){
//    reWheel++;
//    if(reWheel>10){
//      $('.scrollable-sections li.cur').next().find('a').trigger('mousedown');
//      $('.content-up-suggest,.content-down-suggest').remove();
//      reWheel=0;
//    }
//    if(!$('.content-down-suggest').length){
//      $('<div/>').addClass('content-down-suggest').appendTo(this).css({top:this.scrollHeight-130});
//    }else{
//      $('.content-down-suggest').css('opacity', reWheel/10).html(10-reWheel);
//    }
//  }else{
//    reWheel = 0;
//    $('.content-up-suggest,.content-down-suggest').remove();
//  }
//});

//$('.scroll-area').delegate('li a', 'mousedown', function(e) {
//  if ($('.animating').length) {
//    return;
//  }
//  $('.content-up-suggest,.content-down-suggest').remove();
//  window.last_date_link = e.target;
//  $('.browsing-header').show();
//  $('.search-header').hide();
//  var fromLi = $(e.target).parents('.scroll-area').find('li.cur'),
//  newLi = $(e.target).parent();

//  if (fromLi.length && fromLi[0] != newLi[0] && fromLi.is(':visible') && false) {
//    var proxyLi = $('<div></div>').addClass('proxy-li').appendTo(newLi.parent()),
//    fromOff = fromLi.position(),
//    newOff = newLi.position();
//    newLi.addClass('justcur');
//    fromLi.removeClass('cur').removeClass('justcur');
//    proxyLi.css({ left:fromOff.left, top:fromOff.top, width:fromLi.width(), height:fromLi.height() });

//    setTimeout(function() {
//      proxyLi.addClass('animating');
//      proxyLi.on('webkitTransitionEnd', function() {
//        proxyLi.remove();
//        newLi.addClass('cur');
//      });
//      proxyLi.css({ top:newOff.top });
//    });

//  } else {
//      fromLi.removeClass('cur').removeClass('justcur');
//      newLi.addClass('cur').addClass('justcur');
//  }

//  var strDate = getHeaderDate(new Date(Date.parse($(e.target).attr('date'))));

//  $('#search-field').val('');
//  historys.render();

//  if (fromLi.length && fromLi[0] != newLi[0]) {
//    var fromDate = Date.parse(fromLi.find(':first-child').attr('date')),
//    toDate = Date.parse(newLi.find(':first-child').attr('date')),
//    oldContent = $('<div></div>').html($('.content').html()).addClass('content'),
//    curContent = $('.content');

//    var slideDistance = 20,
//    ANIMATION_TIME = $('.debug-time input').val()*1 || 400,
//    ANIMATION_EASING = $('.debug-easing').val(),
//    ANIMATION_DIRECTION = true;

//    function animationDirection(fromDate, toDate) {
//      return ANIMATION_DIRECTION ? (fromDate > toDate) : (fromDate < toDate);
//    }

//    $('.browsing-header').html(strDate);

//    if(animationDirection(fromDate,toDate)) {
//      curContent.before(oldContent);
//      oldContent.css({top:0, opacity:1});
//      curContent.css({top: curContent.height(),opacity:0});
//      oldContent.addClass('animating');
//      curContent.addClass('animating');

//      oldContent.animate({top: -oldContent.height(),opacity:0}, ANIMATION_TIME, ANIMATION_EASING, function(){
//        oldContent.remove();
//      });
//      curContent.animate({top: 0,opacity:1}, ANIMATION_TIME, ANIMATION_EASING, function(){
//        curContent.removeClass('animating');
//      });


//    } else {
//      curContent.after(oldContent);
//      oldContent.css({top:0, opacity:1});
//      curContent.css({top: -curContent.height(),opacity:0});
//      oldContent.addClass('animating');
//      curContent.addClass('animating');

//      oldContent.animate({top: oldContent.height(),opacity:0}, ANIMATION_TIME, ANIMATION_EASING, function(){
//        oldContent.remove();
//      });
//      curContent.animate({top: 0,opacity:1}, ANIMATION_TIME, ANIMATION_EASING, function(){
//        curContent.removeClass('animating');
//      });

//    }
//  } else {
//    $('.browsing-header').html(strDate);
//  }

//});

//$('.sortby').on('click', function(e) {
//  location.hash = e.target.value;
//  historys.render();
//});


//$(document).delegate('.cls-btn', 'click', function(e) {
//  var clsBtn = this;
//  if ($(this).attr('data-all-time')) {
//    historyApis.removeVisits(
//        { url: $(this).attr('url'),
//          timestamps: JSON.parse($(this).attr('data-all-time'))
//        },
//        function() {
//          $(clsBtn).parents('.li-content').addClass('deleted').parent().addClass('del');
//          if (!$(clsBtn).parents('ul').find('>li:not(.del)').length) {
//            $('#clear-pages').attr('disabled', true);
//          }
//        },
//        'deleteComplete'
//    );
//  }
//});

//$(document).delegate('.cls-grp-btn', 'click', function() {
//  var members = $(this).parent().nextUntil('h2');
//  members.addClass('del').find('.li-content').addClass('deleted');
//  $(this).parent().addClass('deleted');

//  if (!$(this).parents('ul').find('>li:not(.del)').length) {
//    $('#clear-pages').attr('disabled', true);
//  }
//  var rv_dict = [];
//  members.find('.cls-btn').each(function(index, el) {
//    rv_dict.push({ url: $(el).attr('url'),
//                   timestamps: JSON.parse($(el).attr('data-all-time'))
//                 });
//  });
//  if (rv_dict.length > 0) {
//    historyApis.removeVisits.apply(historyApis, rv_dict);
//  }
//});

//window.deleteFailed = function() {
//  console.log(arguments);
//};

//$(document).delegate('.search_this', 'click', function(e) {
//  var domain = $(e.target).attr('domain');
//  $('#search-field').val(domain);
//  $('#search-field').removeClass('empty-text');
//  $('#btn-search').trigger('click');
//});


//$('#search-field').blur(function() {
//  if (this.value) {
//    $(this).removeClass('empty-text');
//  } else {
//    $(this).addClass('empty-text');
//  }
//});

//$('#btn-search').on('click', function() {
//  var search_text = $('#search-field').val();
//  if (search_text) {
//    $('.browsing-header').hide();
//    $('.search-header').show();
//    $('.search-header span').html("页面上包含“" + htmlEncode(search_text) + "”的网页");
//    $('.scrollable-sections ol li').removeClass('cur');
//    historys.render();
//  } else {
//    $(window.last_date_link).trigger('mousedown');
//  }
//  return false;
//});

//$('#clear-more').on('click', function() {
//  historyApis.clearBrowsingData();
//});

//$('#clear-pages').on('click', function(e) {
//  if ($('.search-header').is(':visible')) {
//    $('.by-search').show();
//    $('.by-day').hide();
//  } else {
//    $('.by-search').hide();
//    $('.by-day').show();
//    var date = new Date(Date.parse($('.scrollable-sections .cur a').attr('date')));
//    $('.rm-month').html(date.getMonth()+1);
//    $('.rm-day').html(date.getDate());
//  }
//  $('.mask').fadeIn();
//  $('#rm-confirm').show();
//});

//$('#rm-confirm .green-btn').on('click', function() {
//  if ($('.search-header').is(':visible')) {
//    var delTimer = setInterval(function() {
//      var noDels = $('.li-content:not(.deleted) .cls-btn');
//      if (noDels.length) {
//        noDels.eq(0).trigger('click');
//      } else {
//        clearInterval(delTimer);
//        $('#clear-pages').attr('disabled', true);
//        $('.site-sort').addClass('deleted');
//      }
//    }, 50);
//  } else {
//    var toBeRemoved = [];
//    $('.content ul .cls-btn').each(function(idx,item) {
//      if ($(item).attr('data-all-time')) {
//        toBeRemoved.push({
//          url: $(item).attr('url'),
//          timestamps: JSON.parse($(item).attr('data-all-time'))
//        });
//      }
//    });
//    historyApis.removeVisits.apply(historyApis, toBeRemoved.concat(function() {
//      $('.content ul .li-content').each(function(idx,item) {
//        $(item).addClass('deleted');
//      });
//      $('#rm-confirm').hide();
//      $('.mask').fadeOut();
//      $('#clear-pages').attr('disabled', true);
//      $('.site-sort').addClass('deleted');
//    }, 'deleteComplete'));
//  }
//});

//$('#rm-confirm .gray-btn, #rm-confirm .btn-close').on('click', function() {
//  $('#rm-confirm').hide();
//  $('.mask').fadeOut();
//});

//$('.mask').on('selectstart mousedown mousemove', false);

//$(document).on('click', function(e) {
//  if (!$(e.target).parents('#rm-confirm').length && e.target.id != 'clear-pages') {
//    $('#rm-confirm').hide();
//    $('.mask').fadeOut();
//  } else {
//    e.preventDefault();
//    e.stopPropagation();
//  }
//});

//$('.scroll-area').on('contextmenu', function(e) {
//  e.preventDefault();
//});

//$(document).delegate('a[href=#]', 'click', function(e) {
//  e.preventDefault();
//});

//$(document).delegate('.link-tit a', 'click', function(e) {
//  var el = e.target;
//  if ((el.protocol == 'file:' || el.protocol == 'about:') &&
//      (e.button == 0 || e.button == 1)) {
//      chrome.send('navigateToUrl', [
//        el.href,
//        el.target,
//        e.button,
//        e.altKey,
//        e.ctrlKey,
//        e.metaKey,
//        e.shiftKey
//      ]);
//      e.preventDefault();
//  }
//});

//function historyEffct(){
//  var active,$h2A = $('.scroll-area').find('h2 a');
//  $(window).resize(function(){
//    var $win = $(this),
//      $header = $('header'),
//      $scrollArea = $('.scroll-area'),
//      $scrollableSections =$scrollArea.children(),
//      $content = $('.content-wrap'),
//      $topShadow = $('#scrollable-sections-top-shadow'),
//      $bottomShadow = $('#scrollable-sections-bottom-shadow'),
//      winH = $win.height(),
//      headerH = $header.outerHeight(true),
//      scrollAreaH = $scrollArea.outerHeight(true),
//      contentH = $content.outerHeight(true),
//      scrollableSectionsH = $scrollableSections.height(),
//      scollableH = scrollableSectionsH-(winH-headerH-20),
//      sclVal,
//      shadowVal;
//    scrollAreaH > winH-headerH ? $scrollArea.css({'height':winH-headerH-10}) && $content.css({'height':winH-headerH-10}):$scrollArea.css({'height':winH-headerH-10})&&$content.css({'height':winH-headerH-10});

//      $scrollArea.scroll(function(){
//        if( !active ){
//          active = true;
//          sclVal = $(this).scrollTop();
//          shadowVal = sclVal/scollableH;
//          $topShadow.css({'opacity':shadowVal});
//          $bottomShadow.css({'opacity':1-shadowVal});
//          active = false;
//        }
//      });

//  }).trigger("resize");

//  $h2A.click(function(e){
//    if( !active ){
//      active = true;

//      var self = this;

//      $(this).toggleClass('cur').parent().siblings('h2').children().removeClass('cur');
//      $(this).parent().next('ol').slideToggle(function(){
//        active = false;
//      }).siblings('ol').slideUp(function(){

//        $(self).parents('.scroll-area').animate(
//          {scrollTop: self.parentNode.offsetTop});
//      });


//      return false;
//    }

//  });
//}

//historyApis.getHasHistoryDays(60, function(obj, offDays){
//  var days = {}, ym;
//  var currentTime = Date.now();

//  offDays.forEach(function(offDay) {
//    var date = new Date(currentTime + offDay * MillisecondsOfOneDay);
//    ym = date.getFullYear() + '-' + date.getMonth();
//    if (!days[ym]) {
//      days[ym] = [];
//    }
//    days[ym].push( [date.getDate(), offDay, date] );
//  });

//  var h2, ol;
//  calenders = $('.scrollable-sections');
//  calenders.empty();

//  for (var ym in days) {
//    ol = $('<ol></ol>');
//    days[ym].forEach(function(dayUnion) {
//      var day = dayUnion[0],
//          offDay = dayUnion[1],
//          date = dayUnion[2],
//          strDate = getCnDate(date, {
//            month:true,
//            day:true
//          });

//        ol.prepend('<li><a href="#" date="' + (ym.replace(/(\d+)$/,function($,$1){return $1*1+1})+'-'+day)+'" offday="'+offDay+'" title="'+strDate+'">'+day+'</a></li>');
//      });
//      calenders.prepend(ol);
//      calenders.prepend('<h2><a href="#">'+(ym.replace(/\d+?-/g,'')*1+1)+'月</a></h2>');
//    }
//    calenders.find('ol:first').show();
//    calenders.find('h2:first>a').addClass('cur');

//    historyEffct();
//    calenders.find('ol:first li:first a').trigger('mousedown');
//}, 'getHasHistoryDaysComplete');
