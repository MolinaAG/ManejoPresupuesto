﻿using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, 
            IServiciosUsuarios serviciosUsuarios) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if(!ModelState.IsValid) 
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var yaExisteIipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteIipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), 
                                        $"El nombre {tipoCuenta.Nombre} ya existe.");
                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if(tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Borrar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");   
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if(yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) => 
                new TipoCuenta() { Id = valor, Orden = indice + 1}).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
