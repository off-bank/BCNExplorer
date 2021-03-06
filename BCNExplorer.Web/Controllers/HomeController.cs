﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Block;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class HomeController : Controller
    {
        private readonly IBlockService _blockService;

        public HomeController(IBlockService blockService)
        {
            _blockService = blockService;
        }

        public async Task<ActionResult> Index()
        {
            var lastBlock = await _blockService.GetLastBlockHeaderAsync();
            return View(LastBlockViewModel.Create(lastBlock));
        }

        public string Version()
        {
            return App.Version;
        }
    }
}